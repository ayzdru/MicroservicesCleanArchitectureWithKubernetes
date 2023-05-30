using CleanArchitecture.Services.Catalog.API.Entities;
using CleanArchitecture.Shared.DataProtection.Redis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.DataEncryption;
using Microsoft.EntityFrameworkCore.DataEncryption.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CleanArchitecture.Services.Catalog.API.Data
{
    public class CatalogDbContext : DbContext
    {
        private readonly IEncryptionProvider _encryptionProvider;
        public CatalogDbContext(DbContextOptions<CatalogDbContext> options)
              : base(options)
        {
            var kekAesProvider = RedisConnections.GetAesProvider(() => RedisConnections.KekRedisConnection.GetDatabase(), RedisConnections.KeyEncryptionKeyRedisKey);
            _encryptionProvider = RedisConnections.GetAesProvider(() => RedisConnections.DekRedisConnection.GetDatabase(), RedisConnections.DataEncryptionKeyRedisKey, kekAesProvider);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var userEntityBuilder = modelBuilder.Entity<User>();
            userEntityBuilder.Property(x => x.UserName).IsEncrypted();
            userEntityBuilder.Property(x => x.Email).IsEncrypted();

            modelBuilder.UseEncryption(_encryptionProvider);
        }
        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
