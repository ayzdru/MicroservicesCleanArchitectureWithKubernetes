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
    public RedisXmlRepository()
    {
         
    }
    public IReadOnlyCollection<XElement> GetAllElements()
    {
        return GetAllElementsCore().ToList().AsReadOnly();
    }
   
    private IEnumerable<XElement> GetAllElementsCore()
    {        
        var cacheRedisDatabase = RedisConnections.CacheRedisConnection.GetDatabase();        
        var kekAesProvider = RedisConnections.GetKekAesProvider();
        var dekAesProvider = RedisConnections.GetDekAesProvider(kekAesProvider);

        foreach (var value in cacheRedisDatabase.ListRange(RedisConnections.DataProtectionKeysRedisKey))
        {
            yield return XElement.Parse(dekAesProvider.Decrypt((string)value!));
        }
    }

    public void StoreElement(XElement element, string friendlyName)
    {
        var cacheRedisDatabase = RedisConnections.CacheRedisConnection.GetDatabase();
        var kekAesProvider = RedisConnections.GetKekAesProvider();
        var dekAesProvider = RedisConnections.GetDekAesProvider(kekAesProvider);
        cacheRedisDatabase.ListRightPush(RedisConnections.DataProtectionKeysRedisKey, dekAesProvider.Encrypt(element.ToString(SaveOptions.DisableFormatting)));
    }
}