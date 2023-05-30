using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Xml.Linq;
using CleanArchitecture.Shared.DataProtection.Redis;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.EntityFrameworkCore.DataEncryption.Providers;
using StackExchange.Redis;

namespace Microsoft.AspNetCore.DataProtection.StackExchangeRedis;

public class RedisXmlRepository : IXmlRepository
{
    private readonly Func<IDatabase> _cacheDatabaseFactory;
    private readonly Func<IDatabase> _kekDatabaseFactory;
    private readonly Func<IDatabase> _dekDatabaseFactory;
    public RedisXmlRepository(Func<IDatabase> databaseFactory, Func<IDatabase> kekDatabaseFactory, Func<IDatabase> dekDatabaseFactory, RedisKey dataProtectionKeysRedisKey, RedisKey keyEncryptionKeyRedisKey, RedisKey dataEncryptionKeyRedisKey)
    {
        _cacheDatabaseFactory = databaseFactory;
        _kekDatabaseFactory = kekDatabaseFactory;
        _dekDatabaseFactory =   dekDatabaseFactory;
        RedisConnections.DataProtectionKeysRedisKey = dataProtectionKeysRedisKey;
        RedisConnections.KeyEncryptionKeyRedisKey  = keyEncryptionKeyRedisKey;
        RedisConnections.DataEncryptionKeyRedisKey = dataEncryptionKeyRedisKey;
    }

    public IReadOnlyCollection<XElement> GetAllElements()
    {
        return GetAllElementsCore().ToList().AsReadOnly();
    }
    private AesProvider GetKekAesProvider()
    {
        return RedisConnections.GetAesProvider(_kekDatabaseFactory, RedisConnections.KeyEncryptionKeyRedisKey);
    }
    private AesProvider GetDekAesProvider(AesProvider kekAesProvider)
    {
        return RedisConnections.GetAesProvider(_dekDatabaseFactory, RedisConnections.DataEncryptionKeyRedisKey, kekAesProvider);
    }
    private IEnumerable<XElement> GetAllElementsCore()
    {        
        var cacheRedisDatabase = _cacheDatabaseFactory();        
        var kekAesProvider = GetKekAesProvider();
        var dekAesProvider = GetDekAesProvider(kekAesProvider);

        foreach (var value in cacheRedisDatabase.ListRange(RedisConnections.DataProtectionKeysRedisKey))
        {
            yield return XElement.Parse(dekAesProvider.Decrypt((string)value!));
        }
    }

    public void StoreElement(XElement element, string friendlyName)
    {
        var cacheRedisDatabase = _cacheDatabaseFactory();
        var kekAesProvider = GetKekAesProvider();
        var dekAesProvider = GetDekAesProvider(kekAesProvider);
        cacheRedisDatabase.ListRightPush(RedisConnections.DataProtectionKeysRedisKey, dekAesProvider.Encrypt(element.ToString(SaveOptions.DisableFormatting)));
    }
}