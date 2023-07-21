using Consul.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Identity.Web;

namespace CleanArchitecture.Web.BlazorWebAssembly
{
    public class Program
    {
        public static void Main(string[] args)
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
            builder.Services.AddRazorPages();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();


            app.MapRazorPages();
            app.MapFallbackToFile("index.html");

            app.Run();
        }
    }
}