using CleanArchitecture.Services.Catalog.Application.Interfaces;
using CleanArchitecture.Services.Catalog.Core.Entities;
using CleanArchitecture.Services.Catalog.Infrastructure.Extensions;
using CleanArchitecture.Services.Catalog.Infrastructure.Interceptors;
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

namespace CleanArchitecture.Services.Catalog.API.Data
{
    public class CatalogDbContext : DbContext, ICatalogDbContext
    {
        private readonly IEncryptionProvider _encryptionProvider;
        private readonly IMediator _mediator;
        private readonly EntitySaveChangesInterceptor _entitySaveChangesInterceptor;
        public CatalogDbContext(DbContextOptions<CatalogDbContext> options, IMediator mediator, EntitySaveChangesInterceptor entitySaveChangesInterceptor)
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
        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
