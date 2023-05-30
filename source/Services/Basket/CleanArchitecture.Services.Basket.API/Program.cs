using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection.PortableExecutable;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Services.Basket.API.Grpc;
using CleanArchitecture.Services.Catalog.API.Grpc;
using CleanArchitecture.Shared.DataProtection.Redis;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using StackExchange.Redis;

namespace CleanArchitecture.Services.Basket.API
{
    public class Program
    {
        
        public static void Main(string[] args)
        {
            ServicePointManager.ServerCertificateValidationCallback +=
(sender, cert, chain, sslPolicyErrors) => true;
            IdentityModelEventSource.ShowPII = true;


            var builder = WebApplication.CreateBuilder(args);
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
                options.Audience = "basket";
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
            builder.Services.AddHttpContextAccessor();



            var catalogUrl = builder.Configuration.GetValue<string>("CatalogUrl");
            builder.Services.AddScoped<ITokenProvider, AppTokenProvider>();

            builder.Services.AddGrpcClient<Product.ProductClient>(nameof(Product.ProductClient), o =>
            {
                o.Address = new Uri(catalogUrl);
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
            if (builder.Environment.IsDevelopment() == false)
            {
                var cacheRedisConnectionString = builder.Configuration.GetValue<string>("CacheRedisConnectionString");       
                var kekRedisConnectionString = builder.Configuration.GetValue<string>("KeyEncryptionKeyRedisConnectionString");  
                var dekRedisConnectionString = builder.Configuration.GetValue<string>("DataEncryptionKeyRedisConnectionString");
                RedisConnections.SetCacheRedisConnection(cacheRedisConnectionString);
                RedisConnections.SetKekRedisConnection(kekRedisConnectionString);
                RedisConnections.SetDekRedisConnection(dekRedisConnectionString);
                builder.Services.AddRedis(serviceName, RedisConnections.CacheRedisConnection, RedisConnections.KekRedisConnection, RedisConnections.DekRedisConnection);
            }
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
            .AddGrpcCoreInstrumentation();
        if (builder.Environment.IsDevelopment() == true)
        {
            t.AddConsoleExporter();
            builder.Services.AddDistributedMemoryCache();
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

            var app = builder.Build();

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

            app.MapGrpcService<BasketService>().RequireCors("CorsPolicy").EnableGrpcWeb();

            app.MapGet("/", async context =>
            {
                await context.Response.WriteAsync("Basket MicroService");
            });           
            app.Run();
        }
    }
}
