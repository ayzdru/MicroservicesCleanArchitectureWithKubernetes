using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CleanArchitecture.Services.Order.API.Data;
using CleanArchitecture.Services.Order.API.Grpc;
using CleanArchitecture.Services.Order.API.Interfaces;
using CleanArchitecture.Services.Order.API.Services;
using CleanArchitecture.Shared.DataProtection.Redis;
using CleanArchitecture.Shared.HealthChecks;
using Confluent.Kafka.Extensions.OpenTelemetry;
using DotNetCore.CAP.Messages;
using Grpc.Net.Client;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using Npgsql;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using StackExchange.Redis;

namespace CleanArchitecture.Services.Order.API
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
            var healthChecks = builder.Services.AddAllHealthChecks();
            var connectionString = builder.Configuration.GetConnectionString("OrderConnectionString");
            builder.Services.AddDbContext<OrderDbContext>(options =>
                   options.UseNpgsql(connectionString));

            var optionsBuilder = new DbContextOptionsBuilder<OrderDbContext>();
            optionsBuilder.UseNpgsql(connectionString);
            using (var dbContext = new OrderDbContext(optionsBuilder.Options))
            {
                dbContext.Database.Migrate();
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
                options.Audience = "order";
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
                options.EnableDetailedErrors = true;
                options.MaxReceiveMessageSize = 2 * 1024 * 1024; // 2 MB
                options.MaxSendMessageSize = 5 * 1024 * 1024; // 5 MB
            });
            builder.Services.AddResponseCompression(opts =>
            {
                opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { "application/octet-stream" });
            });
            var kafkaConnectionString = builder.Configuration.GetValue<string>("Kafka");
            builder.Services.AddTransient<ISubscriberService, SubscriberService>();
            builder.Services.AddCap(x =>
            {
                x.UseEntityFramework<OrderDbContext>();
                x.UseDashboard();
                x.UseKafka(kafkaConnectionString);
                x.FailedRetryCount = 5;
            });

            builder.Services.AddHttpContextAccessor();


            var basketUrl = builder.Configuration.GetValue<string>("BasketUrl");

            builder.Services.AddScoped<ITokenProvider, AppTokenProvider>();

            builder.Services.AddGrpcClient<CleanArchitecture.Services.Basket.API.Grpc.Basket.BasketClient>(nameof(CleanArchitecture.Services.Basket.API.Grpc.Basket.BasketClient), o =>
            {
                o.Address = new Uri(basketUrl);
                o.ChannelOptionsActions.Add((opt) =>
                {
                    opt.UnsafeUseInsecureChannelCallCredentials = true;
                });
            })
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        return new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                              (message, certificate, chain, sslPolicyErrors) => true
        };
    }).AddCallCredentials(async (context, metadata, serviceProvider) =>
    {
        var provider = serviceProvider.GetRequiredService<ITokenProvider>();
        var token = await provider.GetTokenAsync();
        if (string.IsNullOrEmpty(token) == false)
        {
            metadata.Add("Authorization", $"Bearer {token}");
        }
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
            .AddCapInstrumentation()
            .AddGrpcClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation()
            .AddGrpcCoreInstrumentation().AddNpgsql().AddConfluentKafkaInstrumentation();

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
                .AddDbContextCheck<OrderDbContext>("EntityFrameworkDbContext", HealthStatus.Unhealthy, new string[] { "entityframework", HealthCheckExtensions.Readiness })
                .AddKafka(new Confluent.Kafka.ProducerConfig() { BootstrapServers = kafkaConnectionString }, null, "Kafka", null, new string[] { "kafka", HealthCheckExtensions.Readiness }, HealthCheckExtensions.DefaultTimeOut)
                .AddIdentityServer(new Uri(identityUrl), "IdentityServer", HealthStatus.Unhealthy, new string[] { "identityserver", HealthCheckExtensions.Readiness }, HealthCheckExtensions.DefaultTimeOut);
            var app = builder.Build();

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

            app.MapGrpcService<OrderService>().RequireCors("CorsPolicy").EnableGrpcWeb();

            app.MapGet("/", async context =>
            {
                await context.Response.WriteAsync("Order MicroService");
            });

            app.Run();
        }
    }
}
