using CleanArchitecture.Services.Catalog.API.Data;
using CleanArchitecture.Services.Catalog.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CleanArchitecture.Services.Catalog.Infrastructure.Data;

public class CatalogDbDbContextInitialiser
{
    private readonly ILogger<CatalogDbDbContextInitialiser> _logger;
    private readonly CatalogDbContext _context;

    public CatalogDbDbContextInitialiser(ILogger<CatalogDbDbContextInitialiser> logger, CatalogDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task InitialiseAsync()
    {
        try
        {
            await _context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            if (!_context.Products.Any())
            {
                for (int i = 0; i < 10; i++)
                {
                    var product = new Product( "Product - " + i.ToString(), "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.",  10 * i);
                    product.Create(null);
                    _context.Products.Add(product);
                }
                await _context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }
}
