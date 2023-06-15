using CleanArchitecture.Services.Payment.Application.Interfaces;
using CleanArchitecture.Services.Payment.Core.Entities;
using CleanArchitecture.Services.Payment.Infrastructure.Extensions;
using CleanArchitecture.Services.Payment.Infrastructure.Interceptors;
using CleanArchitecture.Shared.DataProtection.Redis;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.DataEncryption;
using Microsoft.EntityFrameworkCore.DataEncryption.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CleanArchitecture.Services.Payment.Infrastructure.Data
{
    public class PaymentDbContext : DbContext, IPaymentDbContext
    {
        private readonly IEncryptionProvider _encryptionProvider;
        private readonly IMediator _mediator;
        private readonly EntitySaveChangesInterceptor _entitySaveChangesInterceptor; 
        public DbSet<User> Users { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Core.Entities.Payment> Payments { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
        public PaymentDbContext(DbContextOptions<PaymentDbContext> options, IMediator mediator, EntitySaveChangesInterceptor entitySaveChangesInterceptor)
              : base(options)
        {
            _mediator = mediator;
            _entitySaveChangesInterceptor = entitySaveChangesInterceptor;
            if (RedisConnections.KekRedisConnection != null && RedisConnections.DekRedisConnection != null)
            {
                var kekAesProvider = RedisConnections.GetKekAesProvider();
                _encryptionProvider = RedisConnections.GetDekAesProvider(kekAesProvider);
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (RedisConnections.KekRedisConnection != null && RedisConnections.DekRedisConnection != null)
            {
                var userEntityBuilder = modelBuilder.Entity<User>();
                userEntityBuilder.Property(x => x.UserName).IsEncrypted();
                userEntityBuilder.Property(x => x.Email).IsEncrypted();

                modelBuilder.UseEncryption(_encryptionProvider);
            }
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);

        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.AddInterceptors(_entitySaveChangesInterceptor);
        }
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _mediator.DispatchNotifications(this);

            return await base.SaveChangesAsync(cancellationToken);
        }
       
    }
}
