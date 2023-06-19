using CleanArchitecture.Services.Identity.Application.Interfaces;

using CleanArchitecture.Services.Identity.Infrastructure.Extensions;
using CleanArchitecture.Services.Identity.Infrastructure.Interceptors;
using CleanArchitecture.Shared.DataProtection.Redis;
using Duende.IdentityServer.EntityFramework.Options;
using MediatR;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.DataEncryption;
using Microsoft.EntityFrameworkCore.DataEncryption.Providers;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CleanArchitecture.Services.Identity.Infrastructure.Data
{
    public class IdentityDbContext : ApiAuthorizationDbContext<IdentityUser>, IIdentityDbContext
    {
        private readonly IEncryptionProvider _encryptionProvider;
        private readonly IMediator _mediator;
        private readonly EntitySaveChangesInterceptor _entitySaveChangesInterceptor;
        public IdentityDbContext(DbContextOptions<IdentityDbContext> options ,IOptions<OperationalStoreOptions> operationalStoreOptions, IMediator mediator, EntitySaveChangesInterceptor entitySaveChangesInterceptor)
              : base(options, operationalStoreOptions)
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
                var userEntityBuilder = modelBuilder.Entity<IdentityUser>();
                userEntityBuilder.Property(x => x.UserName).IsEncrypted();
                userEntityBuilder.Property(x => x.NormalizedUserName).IsEncrypted();
                userEntityBuilder.Property(x => x.Email).IsEncrypted();
                userEntityBuilder.Property(x => x.NormalizedEmail).IsEncrypted();
                userEntityBuilder.Property(x => x.PasswordHash).IsEncrypted();
                userEntityBuilder.Property(x => x.PhoneNumber).IsEncrypted();

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
