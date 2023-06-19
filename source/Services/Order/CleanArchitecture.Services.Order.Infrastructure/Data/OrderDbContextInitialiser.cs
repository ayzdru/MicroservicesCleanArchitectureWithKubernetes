using CleanArchitecture.Services.Order.Infrastructure.Data;
using CleanArchitecture.Services.Order.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CleanArchitecture.Services.Order.Infrastructure.Data;

public class OrderDbContextInitialiser
{
    private readonly ILogger<OrderDbContextInitialiser> _logger;
    private readonly OrderDbContext _context;

    public OrderDbContextInitialiser(ILogger<OrderDbContextInitialiser> logger, OrderDbContext context)
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
            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }
}
