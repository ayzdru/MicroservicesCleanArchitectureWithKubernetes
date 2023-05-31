using System;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.StackExchangeRedis;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace CleanArchitecture.Shared.DataProtection.Redis
{
    public static class StackExchangeRedisDataProtectionBuilderExtensions
    {
        private const string DataProtectionKeysName = "DataProtection-Keys";
        private const string KeyEncryptionKeyName = "KeyEncryption";
        private const string DataEncryptionKeyName = "DataEncryption";
               
        public static IDataProtectionBuilder PersistKeysToStackExchangeRedis(this IDataProtectionBuilder builder, Func<IDatabase> cacheDatabaseFactory, Func<IDatabase> kekDatabaseFactory, Func<IDatabase> dekDatabaseFactory, RedisKey dataProtectionKeysRedisKey, RedisKey keyEncryptionKeyRedisKey, RedisKey dataEncryptionKeyRedisKey)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(cacheDatabaseFactory);
            return PersistKeysToStackExchangeRedisInternal(builder, cacheDatabaseFactory, kekDatabaseFactory, dekDatabaseFactory, dataProtectionKeysRedisKey, keyEncryptionKeyRedisKey, dataEncryptionKeyRedisKey);
        }

        public static IDataProtectionBuilder PersistKeysToStackExchangeRedis(this IDataProtectionBuilder builder, IConnectionMultiplexer cacheConnectionMultiplexer, IConnectionMultiplexer kekConnectionMultiplexer, IConnectionMultiplexer dekConnectionMultiplexer)
        {
            return PersistKeysToStackExchangeRedis(builder, cacheConnectionMultiplexer, kekConnectionMultiplexer, dekConnectionMultiplexer, DataProtectionKeysName, KeyEncryptionKeyName, DataEncryptionKeyName);
        }

        public static IDataProtectionBuilder PersistKeysToStackExchangeRedis(this IDataProtectionBuilder builder, IConnectionMultiplexer cacheConnectionMultiplexer, IConnectionMultiplexer kekConnectionMultiplexer, IConnectionMultiplexer dekConnectionMultiplexer, RedisKey dataProtectionKeysRedisKey, RedisKey keyEncryptionKeyRedisKey, RedisKey dataEncryptionKeyRedisKey)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(cacheConnectionMultiplexer);
            return PersistKeysToStackExchangeRedisInternal(builder, () => cacheConnectionMultiplexer.GetDatabase(), () => kekConnectionMultiplexer.GetDatabase(), () => dekConnectionMultiplexer.GetDatabase(), dataProtectionKeysRedisKey, keyEncryptionKeyRedisKey, dataEncryptionKeyRedisKey);
        }

        private static IDataProtectionBuilder PersistKeysToStackExchangeRedisInternal(IDataProtectionBuilder builder, Func<IDatabase> cacheDatabaseFactory, Func<IDatabase> kekDatabaseFactory, Func<IDatabase> dekDatabaseFactory, RedisKey dataProtectionKeysRedisKey, RedisKey keyEncryptionKeyRedisKey, RedisKey dataEncryptionKeyRedisKey)
        {
            RedisConnections.DataProtectionKeysRedisKey = dataProtectionKeysRedisKey;
            RedisConnections.KeyEncryptionKeyRedisKey = keyEncryptionKeyRedisKey;
            RedisConnections.DataEncryptionKeyRedisKey = dataEncryptionKeyRedisKey;

            builder.Services.Configure<KeyManagementOptions>(options =>
            {
                options.XmlRepository = new RedisXmlRepository(cacheDatabaseFactory, kekDatabaseFactory, dekDatabaseFactory, dataProtectionKeysRedisKey, keyEncryptionKeyRedisKey, dataEncryptionKeyRedisKey);
            });
            return builder;
        }

        public static IServiceCollection AddRedis(this IServiceCollection services, string serviceName, ConnectionMultiplexer cacheRedisConnection, ConnectionMultiplexer kekRedisConnection, ConnectionMultiplexer dekRedisConnection)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
          

            services.AddSingleton<IConnectionMultiplexer>(sp => cacheRedisConnection);
            services.AddDataProtection().PersistKeysToStackExchangeRedis(cacheRedisConnection, kekRedisConnection, dekRedisConnection);
            services.AddStackExchangeRedisCache(options =>
            {
                options.ConnectionMultiplexerFactory = () => Task.FromResult((IConnectionMultiplexer)cacheRedisConnection);
                options.InstanceName = serviceName;
            });

            return services;
        }
    }
}
