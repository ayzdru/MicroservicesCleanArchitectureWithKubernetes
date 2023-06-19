using CleanArchitecture.Services.Order.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Services.Order.Application.Interfaces
{
    public interface IOrderDbContext
    {
        DbSet<Core.Entities.Order> Orders { get; set; }
        DbSet<OrderItem> OrderItems { get; set; }
        DbSet<Product> Products { get; set; }
        DbSet<User> Users { get; set; }
        DatabaseFacade Database { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}