using Microsoft.EntityFrameworkCore.DataEncryption.Providers;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Shared.DataProtection.Redis
{
    public class RedisConnections
    {
      
        public static RedisKey DataProtectionKeysRedisKey { get; set; }
        public static RedisKey KeyEncryptionKeyRedisKey { get; set; }

        public static RedisKey DataEncryptionKeyRedisKey { get; set; }

        public static RedisKey KeySuffix { get; } = "-Key";
        public static RedisKey IVSuffix { get;} = "-IV";


        private static Lazy<ConnectionMultiplexer> _cacheLazyRedisConnection;

        public static ConnectionMultiplexer CacheRedisConnection
        {
            get
            {
                return _cacheLazyRedisConnection?.Value;
            }
        }
        public static void SetCacheRedisConnection(string cacheRedisConnectionString)
        {
            _cacheLazyRedisConnection = new Lazy<ConnectionMultiplexer>(() =>
            {
                return ConnectionMultiplexer.Connect(cacheRedisConnectionString);
            }, true);
        }
        private static Lazy<ConnectionMultiplexer> _kekLazyRedisConnection;

        public static ConnectionMultiplexer KekRedisConnection
        {
            get
            {

                return _kekLazyRedisConnection?.Value;
            }
        }
        public static void SetKekRedisConnection(string kekRedisConnectionString)
        {
            _kekLazyRedisConnection = new Lazy<ConnectionMultiplexer>(() =>
            {
                return ConnectionMultiplexer.Connect(kekRedisConnectionString);
            }, true);
        }
        private static Lazy<ConnectionMultiplexer> _dekLazyRedisConnection;

        public static ConnectionMultiplexer DekRedisConnection
        {
            get
            {
                return _dekLazyRedisConnection?.Value;
            }
        }
        public static void SetDekRedisConnection(string dekRedisConnectionString)
        {
            _dekLazyRedisConnection = new Lazy<ConnectionMultiplexer>(() =>
            {
                return ConnectionMultiplexer.Connect(dekRedisConnectionString);
            }, true);
        }

        private static AesProvider GetAesProvider(ConnectionMultiplexer connectionMultiplexer, RedisKey redisKey, AesProvider aesProvider = null)
        {
            var database = connectionMultiplexer.GetDatabase();

            var keyRedisKey = redisKey.ToString() + KeySuffix.ToString();
            var key = database.StringGet(keyRedisKey);
            string encryptionKey = aesProvider != null ? aesProvider.Decrypt(key.ToString()) : key.ToString();

            var ivRedisKey = redisKey.ToString() + IVSuffix.ToString();
            var ivKey = database.StringGet(ivRedisKey);
            string ivEncryptionKey = aesProvider != null ? aesProvider.Decrypt(ivKey.ToString()) : ivKey.ToString();
            return new AesProvider(encryptionKey, ivEncryptionKey);
        }
        public static AesProvider GetKekAesProvider()
        {
            return GetAesProvider(KekRedisConnection, KeyEncryptionKeyRedisKey);
        }
        public static AesProvider GetDekAesProvider(AesProvider kekAesProvider)
        {
            return GetAesProvider(DekRedisConnection, DataEncryptionKeyRedisKey, kekAesProvider);
        }
    }
}
