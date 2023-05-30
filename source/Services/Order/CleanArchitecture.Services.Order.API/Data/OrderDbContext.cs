using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CleanArchitecture.Services.Order.API.Entities;
using Microsoft.EntityFrameworkCore.DataEncryption;
using CleanArchitecture.Shared.DataProtection.Redis;

namespace CleanArchitecture.Services.Order.API.Data
{
    public class OrderDbContext : DbContext
    {
        private readonly IEncryptionProvider _encryptionProvider;
        public OrderDbContext(DbContextOptions<OrderDbContext> options)
            : base(options)
        {
            if (RedisConnections.KekRedisConnection != null && RedisConnections.DekRedisConnection != null)
            {
                var kekAesProvider = RedisConnections.GetAesProvider(() => RedisConnections.KekRedisConnection.GetDatabase(), RedisConnections.KeyEncryptionKeyRedisKey);
                _encryptionProvider = RedisConnections.GetAesProvider(() => RedisConnections.DekRedisConnection.GetDatabase(), RedisConnections.DataEncryptionKeyRedisKey, kekAesProvider);
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
        }
        public DbSet<Entities.Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
