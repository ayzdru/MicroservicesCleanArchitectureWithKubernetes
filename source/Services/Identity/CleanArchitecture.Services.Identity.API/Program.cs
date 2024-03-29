using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CleanArchitecture.Services.Identity.API.Services;
using CleanArchitecture.Services.Identity.Core.Interfaces;
using CleanArchitecture.Services.Identity.Infrastructure.Data;
using CleanArchitecture.Shared.DataProtection.Redis;
using CleanArchitecture.Shared.HealthChecks;
using Confluent.Kafka.Extensions.OpenTelemetry;
using DotNetCore.CAP;
using Duende.IdentityServer.EntityFramework.Options;
using Duende.IdentityServer.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using Npgsql;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using CleanArchitecture.Services.Identity.Infrastructure.IoC;
using Consul.AspNetCore;

ServicePointManager.ServerCertificateValidationCallback +=
(sender, cert, chain, sslPolicyErrors) => true;
IdentityModelEventSource.ShowPII = true;
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
if (args.Contains("migrate-database") == true)
{
    var builder = WebApplication.CreateBuilder(args);
    var connectionString = builder.Configuration.GetConnectionString("IdentityConnectionString");
    builder.Services.AddAuthorization();
    builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
      .AddRoles<IdentityRole>()
      .AddEntityFrameworkStores<IdentityDbContext>();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
    builder.Services.AddInfrastructure(builder.Configuration, builder.Environment, connectionString, true);
    var app = builder.Build();
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var initialiser = scope.ServiceProvider.GetRequiredService<IdentityDbContextInitialiser>();
            await initialiser.InitialiseAsync();
            await initialiser.SeedAsync();
            Environment.ExitCode = 0;
            return;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Environment.ExitCode = 1;
            return;
        }
    }
}
else
{
    var builder = WebApplication.CreateBuilder(args);
    var serviceName = builder.Configuration.GetValue<string>("ServiceName");
    builder.Services.AddConsul(serviceName, options =>
    {
        options.Address = new Uri(builder.Configuration.GetValue<string>("Consul"));
    }).AddConsulServiceRegistration(options =>
    {
        options.ID = serviceName;
        options.Name = serviceName;
        options.Address = builder.Configuration.GetValue<string>("ServiceAddress");
        options.Port = builder.Configuration.GetValue<int>("ServicePort");
    });
    var healthChecks = builder.Services.AddAllHealthChecks();
    var connectionString = builder.Configuration.GetConnectionString("IdentityConnectionString");
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
    builder.Services.AddInfrastructure(builder.Configuration, builder.Environment, connectionString);
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders =
            ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    });


    builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<IdentityDbContext>();

    builder.Services.AddIdentityServer(options =>
    {

    })
        //.AddSigningCredentials() Production Environment
        .AddDeveloperSigningCredential()
        .AddApiAuthorization<IdentityUser, IdentityDbContext>();

    builder.Services.AddAuthentication()
        .AddIdentityServerJwt();

    builder.Services.AddControllersWithViews();
    builder.Services.AddRazorPages();
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("CorsPolicy",
            builder => builder
                .AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod().WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding", "Grpc-Accept-Encoding"));
    });
    var kafkaConnectionString = builder.Configuration.GetValue<string>("Kafka");
    builder.Services.AddCap(x =>
    {
        x.UseEntityFramework<IdentityDbContext>();
        x.UseDashboard();
        x.UseKafka(kafkaConnectionString);
        x.FailedRetryCount = 5;
        x.FailedMessageExpiredAfter = int.MaxValue;
    });
   
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
    var identityUrl = builder.Configuration.GetValue<string>("IdentityUrl");
    healthChecks
        .AddNpgSql(connectionString, "SELECT 1;", null, "PostgreSQL", HealthStatus.Unhealthy, new string[] { "postgresql", HealthCheckExtensions.Readiness }, HealthCheckExtensions.DefaultTimeOut)
        .AddDbContextCheck<IdentityDbContext>("EntityFrameworkDbContext", HealthStatus.Unhealthy, new string[] { "entityframework", HealthCheckExtensions.Readiness })
        .AddKafka(new Confluent.Kafka.ProducerConfig() { BootstrapServers = kafkaConnectionString }, null, "Kafka", null, new string[] { "kafka", HealthCheckExtensions.Readiness }, HealthCheckExtensions.DefaultTimeOut);


    var app = builder.Build();
    using (var scope = app.Services.CreateScope())
    {
        var initialiser = scope.ServiceProvider.GetRequiredService<IdentityDbContextInitialiser>();
        await initialiser.InitialiseAsync();
        await initialiser.SeedAsync();
    }
    app.UseAllHealthChecks();
    app.UseForwardedHeaders();
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseHsts();
    }
    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseCors("CorsPolicy");
    app.Use(async (context, next) =>
    {
        //context.Response.Headers.Add("Content-Security-Policy", "script-src 'unsafe-inline'");
        context.SetIdentityServerOrigin(identityUrl);
        await next();
    });

    app.UseForwardedHeaders();
    app.UseIdentityServer();
    app.UseCookiePolicy(new CookiePolicyOptions { MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.Lax });
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapRazorPages();
    app.MapControllers();
    app.MapGet("/", async context =>
    {
        await context.Response.WriteAsync("Identity MicroService");
    });
    app.Run();
}
public partial class Program { }