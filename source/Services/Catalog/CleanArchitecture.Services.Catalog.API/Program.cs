using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CleanArchitecture.Services.Catalog.API.Data;
using CleanArchitecture.Services.Catalog.API.Grpc;
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

            var connectionString = builder.Configuration.GetConnectionString("CatalogConnectionString");
            builder.Services.AddDbContext<CatalogDbContext>(options =>
                   options.UseNpgsql(connectionString));

            var optionsBuilder = new DbContextOptionsBuilder<CatalogDbContext>();
            optionsBuilder.UseNpgsql(connectionString);
            using (var dbContext = new CatalogDbContext(optionsBuilder.Options))
            {
                //if the number of replicas is greater than 1 use Kubernetes Jobs and init containers!
                dbContext.Database.Migrate();      
                if (!dbContext.Products.Any())
                {
                    for (int i = 0; i < 10;i++)
                    {
                        var product = new Entities.Product() { Name = "Product - " + i.ToString(), Price = 10 * i, Description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum." };
                        product.Create(null);
                        dbContext.Products.Add(product);
                    }
                    dbContext.SaveChanges();
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
            .AddGrpcClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation()
            .AddGrpcCoreInstrumentation()
            .AddNpgsql();

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
            app.MapGrpcService<ProductService>().RequireCors("CorsPolicy").EnableGrpcWeb();
            app.MapGet("/", async context =>
            {
                await context.Response.WriteAsync("Catalog MicroService");
            });

            app.Run();
        }
    }
}
