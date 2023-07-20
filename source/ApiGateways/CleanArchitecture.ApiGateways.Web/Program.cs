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

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAllHealthChecks();
builder.Configuration.AddJsonFile("ocelot.json");
if (builder.Environment.IsProduction() == false)
{
    builder.Configuration.AddJsonFile($"ocelot.{builder.Environment.EnvironmentName}.json");
}
var serviceName = builder.Configuration.GetValue<string>("ServiceName");
var server = builder.Services.BuildServiceProvider().GetService<IServer>();

var addresses = server?.Features.Get<IServerAddressesFeature>().Addresses;
builder.Services.AddResponseCompression();
builder.Services.AddConsul(serviceName, options =>
{
    options.Address = new Uri(builder.Configuration.GetValue<string>("Consul"));
}).AddConsulDynamicServiceRegistration(options =>
{
    options.ID = serviceName;
    options.Name = serviceName;
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

