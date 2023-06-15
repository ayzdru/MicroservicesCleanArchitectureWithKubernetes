using CleanArchitecture.Services.Payment.Infrastructure.Data;
using CleanArchitecture.Services.Payment.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CleanArchitecture.Services.Payment.Infrastructure.Data;

public class PaymentDbContextInitialiser
{
    private readonly ILogger<PaymentDbContextInitialiser> _logger;
    private readonly PaymentDbContext _context;

    public PaymentDbContextInitialiser(ILogger<PaymentDbContextInitialiser> logger, PaymentDbContext context)
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
