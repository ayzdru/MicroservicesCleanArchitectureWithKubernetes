using CleanArchitecture.Services.Order.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Services.Order.Application.Interfaces
{
    public interface IPaymentDbContext
    {
        DbSet<Core.Entities.Order> Orders { get; set; }
        DbSet<User> Users { get; set; }
        DbSet<Core.Entities.Payment> Payments { get; set; }
        DbSet<PaymentTransaction> PaymentTransactions { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}