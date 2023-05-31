using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CleanArchitecture.Services.Payment.API.Entities;
using Microsoft.EntityFrameworkCore.DataEncryption;
using CleanArchitecture.Shared.DataProtection.Redis;

namespace CleanArchitecture.Services.Payment.API.Data
{
    public class PaymentDbContext : DbContext
    {
        private readonly IEncryptionProvider _encryptionProvider;
        public PaymentDbContext(DbContextOptions<PaymentDbContext> options)
            : base(options)
        {
            if (RedisConnections.KekRedisConnection != null && RedisConnections.DekRedisConnection != null)
            {
                var kekAesProvider = RedisConnections.GetKekAesProvider();
                _encryptionProvider = RedisConnections.GetDekAesProvider(kekAesProvider);
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            if (RedisConnections.KekRedisConnection != null && RedisConnections.DekRedisConnection != null)
            {
                var userEntityBuilder = modelBuilder.Entity<User>();
                userEntityBuilder.Property(x => x.UserName).IsEncrypted();
                userEntityBuilder.Property(x => x.Email).IsEncrypted();

                modelBuilder.UseEncryption(_encryptionProvider);
            }
        }
        public DbSet<Entities.Payment> Payments { get; set; }
        public DbSet<Entities.User> Users { get; set; }
    }
}
