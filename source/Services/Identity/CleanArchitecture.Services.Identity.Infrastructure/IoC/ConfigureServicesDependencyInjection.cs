using CleanArchitecture.Services.Identity.Application.Interfaces;
using CleanArchitecture.Services.Identity.Application.IoC;
using CleanArchitecture.Services.Identity.Infrastructure.Data;
using CleanArchitecture.Services.Identity.Infrastructure.Interceptors;
using CleanArchitecture.Services.Identity.Infrastructure.Services;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Services.Identity.Infrastructure.IoC
{
    public static class ConfigureServicesDependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment webHostEnvironment, string connectionString)
        {
            services.AddDbContext<IdentityDbContext>(options =>
            {
                options.UseNpgsql(connectionString);
                options.UseTriggers(triggerOptions =>
                {
                    triggerOptions.AddTrigger<UserTriggerService>();
                });
            });
            services.AddScoped<EntitySaveChangesInterceptor>();
            services.AddScoped<IIdentityDbContext>(provider => provider.GetService<IdentityDbContext>());
            if (webHostEnvironment.IsDevelopment())
            {
                services.AddScoped<IdentityDbContextInitialiser>();
            }
            services.AddApplication();
            return services;
        }
    }
}
