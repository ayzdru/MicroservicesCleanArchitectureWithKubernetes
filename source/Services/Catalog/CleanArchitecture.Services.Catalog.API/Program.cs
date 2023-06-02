using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using CleanArchitecture.Services.Catalog.API.Data;
using CleanArchitecture.Services.Catalog.API.Grpc.V1;
using CleanArchitecture.Services.Catalog.API.Interfaces;
using CleanArchitecture.Services.Catalog.API.Services;
using CleanArchitecture.Shared.DataProtection.Redis;
using CleanArchitecture.Shared.HealthChecks;
using Confluent.Kafka.Extensions.OpenTelemetry;
using DotNetCore.CAP;
using DotNetCore.CAP.Messages;
using Google.Protobuf.WellKnownTypes;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.DataEncryption.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using Microsoft.OpenApi.Models;
using Npgsql;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
namespace CleanArchitecture.Services.Catalog.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ServicePointManager.ServerCertificateValidationCallback +=
              (sender, cert, chain, sslPolicyErrors) => true;
            IdentityModelEventSource.ShowPII = true;

            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddGrpcHealthChecks();
            var healthChecks = builder.Services.AddAllHealthChecks();

            var connectionString = builder.Configuration.GetConnectionString("CatalogConnectionString");
            builder.Services.AddDbContext<CatalogDbContext>(options =>
                   options.UseNpgsql(connectionString));


           

            var optionsBuilder = new DbContextOptionsBuilder<CatalogDbContext>();
            optionsBuilder.UseNpgsql(connectionString);
            
            using (var dbContext = new CatalogDbContext(optionsBuilder.Options))
            {
                //if the number of replicas is greater than one use Kubernetes Jobs, init containers!                
                dbContext.Database.Migrate();
                try
                {
                    if (!dbContext.Products.Any())
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            var product = new Entities.Product() { Name = "Product - " + i.ToString(), Price = 10 * i, Description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum." };
                            product.Create(null);
                            dbContext.Products.Add(product);
                        }
                        dbContext.SaveChanges();
                    }                   
                }
                catch
                {

                }
            }

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove("sub");

            var identityUrl = builder.Configuration.GetValue<string>("IdentityUrl");

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(options =>
            {
                options.Authority = identityUrl;
                options.RequireHttpsMetadata = true;
                options.Audience = "catalog";
                options.BackchannelHttpHandler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                              (message, certificate, chain, sslPolicyErrors) => true
                };
            });
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder
                        .AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod().WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding", "Grpc-Accept-Encoding"));
            });
            builder.Services.AddAuthorization();
            builder.Services.AddGrpc(options =>
            {
                if (builder.Environment.IsDevelopment() == true)
                {
                    options.EnableDetailedErrors = true;
                }
                options.MaxReceiveMessageSize = 2 * 1024 * 1024; // 2 MB
                options.MaxSendMessageSize = 5 * 1024 * 1024; // 5 MB
            }).AddJsonTranscoding();
            builder.Services.AddGrpcSwagger();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1",
                    new OpenApiInfo { Title = "gRPC transcoding", Version = "v1" });
                                var filePath = Path.Combine(System.AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
                c.IncludeXmlComments(filePath);
                c.IncludeGrpcXmlComments(filePath, includeControllerXmlComments: true);
                c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri(identityUrl + "/connect/authorize"),
                            TokenUrl = new Uri(identityUrl + "/connect/token"),
                            Scopes = new Dictionary<string, string>
                                {
                                    { "catalog", "Access read/write operations" },
                                }
                        }
                    }
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
                            },
                            new[] { "catalog" }
                        }
                    });
            });
            builder.Services.AddResponseCompression(opts =>
            {
                opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { "application/octet-stream" });
            });
            builder.Services.AddTransient<ISubscriberService, SubscriberService>();
            var kafkaConnectionString = builder.Configuration.GetValue<string>("Kafka");
            builder.Services.AddCap(x =>
            {
                x.UseEntityFramework<CatalogDbContext>();
                x.UseDashboard();
                x.UseKafka(kafkaConnectionString);
                x.FailedRetryCount = 5;
                x.FailedMessageExpiredAfter = int.MaxValue;
            });

            var serviceName = builder.Configuration.GetValue<string>("ServiceName");
            var cacheRedisConnectionString = builder.Configuration.GetValue<string>("CacheRedisConnectionString");
            var kekRedisConnectionString = builder.Configuration.GetValue<string>("KeyEncryptionKeyRedisConnectionString");
            var dekRedisConnectionString = builder.Configuration.GetValue<string>("DataEncryptionKeyRedisConnectionString");

            builder.Services.AddRedis(serviceName, cacheRedisConnectionString, kekRedisConnectionString, dekRedisConnectionString);
            healthChecks.AddRedis(cacheRedisConnectionString, "Cache", HealthStatus.Unhealthy, new string[] { "redis", HealthCheckExtensions.Readiness }, HealthCheckExtensions.DefaultTimeOut);
            healthChecks.AddRedis(kekRedisConnectionString, "KeyEncryptionKey", HealthStatus.Unhealthy, new string[] { "redis", HealthCheckExtensions.Readiness }, HealthCheckExtensions.DefaultTimeOut);
            healthChecks.AddRedis(dekRedisConnectionString, "DataEncryptionKey", HealthStatus.Unhealthy, new string[] { "redis", HealthCheckExtensions.Readiness }, HealthCheckExtensions.DefaultTimeOut);

            var openTelemetryProtocolEndpoint = builder.Configuration.GetValue<string>("OpenTelemetryProtocolEndpoint");
            Action<ResourceBuilder> configureResource = r => r.AddService(
    serviceName: serviceName,
    serviceVersion: typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown",
    serviceInstanceId: Environment.MachineName);
            builder.Services.AddOpenTelemetry()
    .ConfigureResource(configureResource)
    .WithTracing(t =>
    {
        t.AddSource(serviceName)
            .SetSampler(new AlwaysOnSampler())
            .AddHttpClientInstrumentation()
            .AddAspNetCoreInstrumentation()
            .AddGrpcClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation()
            .AddGrpcCoreInstrumentation()
            .AddCapInstrumentation()
            .AddNpgsql().AddConfluentKafkaInstrumentation();

        if (builder.Environment.IsDevelopment() == true)
        {
            t.AddConsoleExporter();
        }
        else
        {
            t.AddRedisInstrumentation(RedisConnections.CacheRedisConnection).AddRedisInstrumentation(RedisConnections.KekRedisConnection).AddRedisInstrumentation(RedisConnections.DekRedisConnection);
            t.AddOtlpExporter(otlpOptions =>
            {
                otlpOptions.Endpoint = new Uri(openTelemetryProtocolEndpoint);
            });
        }
    })
    .WithMetrics(m =>
    {
        m
            .AddMeter(serviceName)
            .SetExemplarFilter(new TraceBasedExemplarFilter())
            .AddRuntimeInstrumentation()
            .AddHttpClientInstrumentation()
            .AddAspNetCoreInstrumentation();
        if (builder.Environment.IsDevelopment() == true)
        {
            m.AddConsoleExporter();
        }
        else
        {
            m.AddOtlpExporter(otlpOptions =>
            {
                otlpOptions.Endpoint = new Uri(openTelemetryProtocolEndpoint);
            });
        }
    });
            builder.Logging.ClearProviders();

            builder.Logging.AddOpenTelemetry(options =>
            {
                var resourceBuilder = ResourceBuilder.CreateDefault();
                configureResource(resourceBuilder);
                options.SetResourceBuilder(resourceBuilder);
                if (builder.Environment.IsDevelopment() == true)
                {
                    options.AddConsoleExporter();
                }
                else
                {
                    options.AddOtlpExporter(otlpOptions =>
                    {
                        otlpOptions.Endpoint = new Uri(openTelemetryProtocolEndpoint);
                    });
                }

            });
            healthChecks
              .AddNpgSql(connectionString, "SELECT 1;", null, "PostgreSQL", HealthStatus.Unhealthy, new string[] { "postgresql", HealthCheckExtensions.Readiness }, HealthCheckExtensions.DefaultTimeOut)
              .AddDbContextCheck<CatalogDbContext>("EntityFrameworkDbContext", HealthStatus.Unhealthy, new string[] { "entityframework", HealthCheckExtensions.Readiness })
              .AddKafka(new Confluent.Kafka.ProducerConfig() { BootstrapServers = kafkaConnectionString }, null, "Kafka", null, new string[] { "kafka", HealthCheckExtensions.Readiness }, HealthCheckExtensions.DefaultTimeOut)
              .AddIdentityServer(new Uri(identityUrl), "IdentityServer", HealthStatus.Unhealthy, new string[] { "identityserver", HealthCheckExtensions.Readiness }, HealthCheckExtensions.DefaultTimeOut);
            var app = builder.Build();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Catalog API V1");
                c.OAuthClientId("CatalogSwagger");
                c.OAuthAppName("CatalogSwagger");
                c.OAuthScopeSeparator(" ");
                c.OAuthUsePkce();
            });
            app.UseAllHealthChecks();
            app.UseResponseCompression();
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseCors("CorsPolicy");
            app.UseGrpcWeb(new GrpcWebOptions
            {
                DefaultEnabled = true
            });

            app.UseAuthentication();
            app.UseAuthorization();
            app.MapGrpcService<ProductServiceV1>().RequireCors("CorsPolicy").EnableGrpcWeb();
            app.MapGrpcHealthChecksService().RequireCors("CorsPolicy").EnableGrpcWeb();
            app.MapGet("/", async context =>
            {
                await context.Response.WriteAsync("Catalog MicroService");
            });

            app.Run();
        }
    }
}
