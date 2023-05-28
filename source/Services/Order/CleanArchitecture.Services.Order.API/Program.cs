using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CleanArchitecture.Services.Order.API.Data;
using CleanArchitecture.Services.Order.API.Grpc;
using DotNetCore.CAP.Messages;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using Npgsql;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

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
            builder.Services.AddGrpc(options => {
                options.EnableDetailedErrors = true;
                options.MaxReceiveMessageSize = 2 * 1024 * 1024; // 2 MB
                options.MaxSendMessageSize = 5 * 1024 * 1024; // 5 MB
            });
            builder.Services.AddResponseCompression(opts =>
            {
                opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { "application/octet-stream" });
            });
            builder.Services.AddCap(x =>
            {
                x.UseEntityFramework<OrderDbContext>();
                x.UseDashboard();
                x.UseRabbitMQ(q=> {
                    q.UserName = builder.Configuration.GetValue<string>("RabbitMQUserName");
                    q.Password = builder.Configuration.GetValue<string>("RabbitMQPassword");
                    q.HostName = builder.Configuration.GetValue<string>("RabbitMQHostName");
                    q.Port = builder.Configuration.GetValue<int>("RabbitMQPort");
                });
                x.FailedRetryCount = 5;
                x.FailedThresholdCallback = failed =>
                {
                    var aaa =
                        $@"A message of type {failed.MessageType} failed after executing {x.FailedRetryCount} several times, 
                        requiring manual troubleshooting. Message name: {failed.Message.GetName()}";
                };
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
            .AddGrpcCoreInstrumentation().AddNpgsql();

        t.AddOtlpExporter(otlpOptions =>
        {
            otlpOptions.Endpoint = new Uri(openTelemetryProtocolEndpoint);
        });
        t.AddConsoleExporter();
    })
    .WithMetrics(m =>
    {
        m
            .AddMeter(serviceName)
            .SetExemplarFilter(new TraceBasedExemplarFilter())
            .AddRuntimeInstrumentation()
            .AddHttpClientInstrumentation()
            .AddAspNetCoreInstrumentation();
        m.AddOtlpExporter(otlpOptions =>
        {
            otlpOptions.Endpoint = new Uri(openTelemetryProtocolEndpoint);
        });
        m.AddConsoleExporter();
    });
            builder.Logging.ClearProviders();

            builder.Logging.AddOpenTelemetry(options =>
            {
                var resourceBuilder = ResourceBuilder.CreateDefault();
                configureResource(resourceBuilder);
                options.SetResourceBuilder(resourceBuilder);
                options.AddOtlpExporter(otlpOptions =>
                {
                    otlpOptions.Endpoint = new Uri(openTelemetryProtocolEndpoint);
                });
                options.AddConsoleExporter();

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

            app.MapGrpcService<OrderService>().RequireCors("CorsPolicy").EnableGrpcWeb();

            app.MapGet("/", async context =>
            {
                await context.Response.WriteAsync("Order MicroService");
            });

            app.Run();
        }
    }
}
