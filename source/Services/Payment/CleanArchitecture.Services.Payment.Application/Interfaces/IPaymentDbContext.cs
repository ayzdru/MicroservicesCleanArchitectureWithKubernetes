using CleanArchitecture.Services.Payment.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Services.Payment.Application.Interfaces
{
    public interface IPaymentDbContext
    {
        DbSet<Order> Orders { get; set; }
        DbSet<User> Users { get; set; }
        DbSet<Core.Entities.Payment> Payments { get; set; }
        DbSet<PaymentTransaction> PaymentTransactions { get; set; }
        DatabaseFacade Database { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}