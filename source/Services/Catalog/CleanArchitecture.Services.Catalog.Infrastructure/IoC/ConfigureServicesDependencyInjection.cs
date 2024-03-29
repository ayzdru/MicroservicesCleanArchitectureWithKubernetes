﻿using CleanArchitecture.Services.Catalog.Infrastructure.Data;
using CleanArchitecture.Services.Catalog.Application;
using CleanArchitecture.Services.Catalog.Application.Behaviours;
using CleanArchitecture.Services.Catalog.Application.Interfaces;
using CleanArchitecture.Services.Catalog.Application.IoC;
using CleanArchitecture.Services.Catalog.Core.Entities;
using CleanArchitecture.Services.Catalog.Core.Interfaces;
using CleanArchitecture.Services.Catalog.Infrastructure.Interceptors;
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

namespace CleanArchitecture.Services.Catalog.Infrastructure.IoC
{
    public static class ConfigureServicesDependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment webHostEnvironment, string connectionString)
        {
            services.AddDbContext<CatalogDbContext>(options =>
                   options.UseNpgsql(connectionString));
            services.AddScoped<EntitySaveChangesInterceptor>();
            services.AddScoped<ICatalogDbContext>(provider => provider.GetService<CatalogDbContext>());
            services.AddScoped<CatalogDbContextInitialiser>();
            services.AddApplication();
            return services;
        }
    }
}
