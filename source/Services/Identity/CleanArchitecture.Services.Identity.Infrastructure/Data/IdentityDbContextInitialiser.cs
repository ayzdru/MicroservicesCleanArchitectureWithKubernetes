using CleanArchitecture.Services.Identity.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CleanArchitecture.Services.Identity.Infrastructure.Data;

public class IdentityDbContextInitialiser
{
    private readonly ILogger<IdentityDbContextInitialiser> _logger;
    private readonly IdentityDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    public IdentityDbContextInitialiser(ILogger<IdentityDbContextInitialiser> logger, IdentityDbContext context, UserManager<IdentityUser> userManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
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
            var administrator = new IdentityUser { UserName = "administrator@localhost", Email = "administrator@localhost", EmailConfirmed = true, PhoneNumberConfirmed = true };

            if (_userManager.Users.All(u => u.UserName != administrator.UserName))
            {
                await _userManager.CreateAsync(administrator, "Password1@!");               
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }
}
