using Consul;
using Consul.AspNetCore;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Consul;
using Ocelot.Values;
using CleanArchitecture.Shared.HealthChecks;
using Kros.Utils;
using System;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAllHealthChecks();
builder.Configuration.AddJsonFile("ocelot.json");
if (builder.Environment.IsProduction() == false)
{
    builder.Configuration.AddJsonFile($"ocelot.{builder.Environment.EnvironmentName}.json");
}
var serviceName = builder.Configuration.GetValue<string>("ServiceName");
var consul = builder.Configuration.GetValue<string>("Consul");
var serviceAddress = builder.Configuration.GetValue<string>("ServiceAddress");
var servicePort = builder.Configuration.GetValue<int>("ServicePort");

builder.Services.AddResponseCompression();
builder.Services.AddConsul(serviceName, options =>
{
    options.Address = new Uri(consul);
}).AddConsulServiceRegistration(options =>
{
    options.ID = serviceName;
    options.Name = serviceName;
    options.Address = serviceAddress;
    options.Port = servicePort;
    options.Check = new AgentCheckRegistration()
    {
        HTTP = $"http://{serviceAddress}:{servicePort}/health",
        Notes = "Checks /health",
        Timeout = TimeSpan.FromSeconds(20),
        Interval = TimeSpan.FromSeconds(60)
    };
});
builder.Services.AddOcelot().AddConsul();
builder.Services.AddControllers();
builder.Services.AddSwaggerForOcelot(builder.Configuration);
var app = builder.Build();
app.MapControllers();
app.MapGet("/", async (IConsulClient consulClient) =>
{
    var allRegisteredServices = await consulClient.Agent.Services();
    return string.Join(",", allRegisteredServices.Response?.Select(x => x.Value.Service + " - (" + x.Value.Address + ")").ToList());
}); 
app.UseResponseCompression();
app.UseAllHealthChecks();
app.UsePathBase("/gateway");
app.UseStaticFiles();
app.UseSwaggerForOcelotUI(opt =>
{
    opt.DownstreamSwaggerEndPointBasePath = "/gateway/swagger/docs";
    opt.PathToSwaggerGenerator = "/swagger/docs";
})
            .UseOcelot()
            .Wait();
app.Run();

