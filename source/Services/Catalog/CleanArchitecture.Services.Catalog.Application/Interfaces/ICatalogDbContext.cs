﻿using CleanArchitecture.Services.Catalog.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Services.Catalog.Application.Interfaces
{
    public interface ICatalogDbContext
    {
        DbSet<User> Users { get; set; }
        DbSet<Product> Products { get; set; }
        DatabaseFacade Database { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}