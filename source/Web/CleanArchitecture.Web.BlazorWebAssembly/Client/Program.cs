using CleanArchitecture.Web.BlazorWebAssembly.Client;
using Grpc.Net.Client.Web;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using CleanArchitecture.Web.BlazorWebAssembly.Client.States;
using CleanArchitecture.Services.Order.API.Grpc;
using CleanArchitecture.Services.Basket.API.Grpc;
using CleanArchitecture.Services.Catalog.API.Grpc;

namespace CleanArchitecture.Web.BlazorWebAssembly.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");
            var identityUrl = builder.Configuration.GetSection("IdentityUrl").Value;
            builder.Services.AddHttpClient("CleanArchitecture.Services.Identity.API", client => client.BaseAddress = new Uri(identityUrl))
                .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

            // Supply HttpClient instances that include access tokens when making requests to the server project
            builder.Services.AddTransient(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("CleanArchitecture.Services.Identity.API"));

            builder.Services.AddApiAuthorization(a =>
            {
                a.ProviderOptions.ConfigurationEndpoint = $"{identityUrl}/_configuration/CleanArchitecture.Web.BlazorWebAssembly";
            });



            builder.Services.AddSingleton(services =>
            {
                // Get the service address from appsettings.json
                var config = services.GetRequiredService<IConfiguration>();
                var catalogUrl = config["CatalogUrl"];

                var httpClient = new HttpClient(new GrpcWebHandler(GrpcWebMode.GrpcWebText, new StreamingHttpHandler(new HttpClientHandler())));
                var channel = GrpcChannel.ForAddress(catalogUrl, new GrpcChannelOptions { HttpClient = httpClient });

                return new Product.ProductClient(channel);
            });
            builder.Services.AddSingleton(services =>
            {
                // Get the service address from appsettings.json
                var config = services.GetRequiredService<IConfiguration>();
                var basketUrl = config["BasketUrl"];

                var httpClient = new HttpClient(new GrpcWebHandler(GrpcWebMode.GrpcWebText, new StreamingHttpHandler(new HttpClientHandler())));
                var channel = GrpcChannel.ForAddress(basketUrl, new GrpcChannelOptions { HttpClient = httpClient });

                return new Basket.BasketClient(channel);
            });
            builder.Services.AddSingleton(services =>
            {
                // Get the service address from appsettings.json
                var config = services.GetRequiredService<IConfiguration>();
                var orderUrl = config["OrderUrl"];

                var httpClient = new HttpClient(new GrpcWebHandler(GrpcWebMode.GrpcWebText, new StreamingHttpHandler(new HttpClientHandler())));
                var channel = GrpcChannel.ForAddress(orderUrl, new GrpcChannelOptions { HttpClient = httpClient });

                return new Order.OrderClient(channel);
            });
            builder.Services.AddSingleton<BasketState>();

            await builder.Build().RunAsync();
        }
    }
}