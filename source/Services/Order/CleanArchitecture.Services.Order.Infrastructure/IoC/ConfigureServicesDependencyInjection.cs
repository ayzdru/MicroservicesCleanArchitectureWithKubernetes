using CleanArchitecture.Services.Order.Application.Interfaces;
using CleanArchitecture.Services.Order.Application.IoC;
using CleanArchitecture.Services.Order.Core.Entities;
using CleanArchitecture.Services.Order.Core.Interfaces;
using CleanArchitecture.Services.Order.Infrastructure.Data;
using CleanArchitecture.Services.Order.Infrastructure.Interceptors;
using CleanArchitecture.Services.Order.Infrastructure.Services;
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

namespace CleanArchitecture.Services.Order.Infrastructure.IoC
{
    public static class ConfigureServicesDependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment webHostEnvironment, string connectionString)
        {
            services.AddTransient<IIdentityService, IdentityService>();
            services.AddDbContext<PaymentDbContext>(options =>
                   options.UseNpgsql(connectionString));
            services.AddScoped<EntitySaveChangesInterceptor>();
            services.AddScoped<IPaymentDbContext>(provider => provider.GetService<PaymentDbContext>());
            if (webHostEnvironment.IsDevelopment())
            {
                services.AddScoped<PaymentDbContextInitialiser>();
            }
            services.AddApplication();
            return services;
        }
    }
}
