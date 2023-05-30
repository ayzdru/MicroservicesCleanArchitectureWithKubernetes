using CleanArchitecture.Shared.DataProtection.Redis;
using Duende.IdentityServer.EntityFramework.Options;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.DataEncryption;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CleanArchitecture.Services.Identity.API.Data
{
    public class IdentityDbContext : ApiAuthorizationDbContext<IdentityUser>
    {
        private readonly IEncryptionProvider _encryptionProvider;
        public IdentityDbContext(
            DbContextOptions options,
            IOptions<OperationalStoreOptions> operationalStoreOptions) : base(options, operationalStoreOptions)
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
                var userEntityBuilder = modelBuilder.Entity<IdentityUser>();
                userEntityBuilder.Property(x => x.UserName).IsEncrypted();
                userEntityBuilder.Property(x => x.NormalizedUserName).IsEncrypted();
                userEntityBuilder.Property(x => x.Email).IsEncrypted();
                userEntityBuilder.Property(x => x.NormalizedEmail).IsEncrypted();
                userEntityBuilder.Property(x => x.PasswordHash).IsEncrypted();
                userEntityBuilder.Property(x => x.PhoneNumber).IsEncrypted();

                modelBuilder.UseEncryption(_encryptionProvider);
            }
        }
    }
}
