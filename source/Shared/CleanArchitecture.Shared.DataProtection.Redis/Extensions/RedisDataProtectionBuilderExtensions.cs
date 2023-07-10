using System;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.StackExchangeRedis;
using Microsoft.EntityFrameworkCore.DataEncryption.Providers;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace CleanArchitecture.Shared.DataProtection.Redis
{
    public static class StackExchangeRedisDataProtectionBuilderExtensions
    {
        private const string DataProtectionKeysName = "DataProtection-Keys";
        private const string KeyEncryptionKeyName = "KeyEncryption";
        private const string DataEncryptionKeyName = "DataEncryption";

        private static IDataProtectionBuilder PersistKeysToStackExchangeRedis(this IDataProtectionBuilder builder)
        {
            
            builder.Services.Configure<KeyManagementOptions>(options =>
            {
                options.XmlRepository = new RedisXmlRepository();
            });
            return builder;
        }

        public static IServiceCollection AddRedis(this IServiceCollection services, string serviceName,string cacheRedisConnectionString, string kekRedisConnectionString,string dekRedisConnectionString)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            


            try
            {
                RedisConnections.DataProtectionKeysRedisKey = DataProtectionKeysName;
                RedisConnections.KeyEncryptionKeyRedisKey = KeyEncryptionKeyName;
                RedisConnections.DataEncryptionKeyRedisKey = DataEncryptionKeyName;

                RedisConnections.SetCacheRedisConnection(cacheRedisConnectionString);
                RedisConnections.SetKekRedisConnection(kekRedisConnectionString);
                RedisConnections.SetDekRedisConnection(dekRedisConnectionString);

                var kek = RedisConnections.KekRedisConnection.GetDatabase();
                if (kek.KeyExists(RedisConnections.KeyEncryptionKeyRedisKey + RedisConnections.KeySuffix) == false)
                {
                    var kekInfo = AesProvider.GenerateKey(AesKeySize.AES256Bits);
                    kek.StringSet(RedisConnections.KeyEncryptionKeyRedisKey + RedisConnections.KeySuffix, kekInfo.Key);
                    kek.StringSet(RedisConnections.KeyEncryptionKeyRedisKey + RedisConnections.IVSuffix, kekInfo.IV);

                    var kekProvider = RedisConnections.GetKekAesProvider();
                    var dekInfo = AesProvider.GenerateKey(AesKeySize.AES256Bits);
                    var dek = RedisConnections.DekRedisConnection.GetDatabase();
                    dek.StringSet(RedisConnections.DataEncryptionKeyRedisKey + RedisConnections.KeySuffix, kekProvider.Encrypt(dekInfo.Key));
                    dek.StringSet(RedisConnections.DataEncryptionKeyRedisKey + RedisConnections.IVSuffix, kekProvider.Encrypt(dekInfo.IV));
                }
            }
            catch 
            {

            }
            
            services.AddSingleton<IConnectionMultiplexer>(sp => RedisConnections.CacheRedisConnection);
            services.AddDataProtection().PersistKeysToStackExchangeRedis();
            services.AddStackExchangeRedisCache(options =>
            {
                options.ConnectionMultiplexerFactory = () => Task.FromResult((IConnectionMultiplexer)RedisConnections.CacheRedisConnection);
                options.InstanceName = serviceName;
            });

            return services;
        }
    }
}
