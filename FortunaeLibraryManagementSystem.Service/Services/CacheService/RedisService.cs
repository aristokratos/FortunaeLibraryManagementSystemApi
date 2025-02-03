
namespace FortunaeLibraryManagementSystem.Service.Services.CacheService;
using StackExchange.Redis;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
public class RedisService : IRedisService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;

    public RedisService(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _db = redis.GetDatabase();
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await _db.StringGetAsync(key);
        if (value.IsNull)
            return default;

        return JsonSerializer.Deserialize<T>(value!);
    }

    public async Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var serializedValue = JsonSerializer.Serialize(value);
        return await _db.StringSetAsync(key, serializedValue, expiration);
    }

    public async Task<bool> RemoveAsync(string key)
    {
        return await _db.KeyDeleteAsync(key);
    }

    public async Task<bool> KeyExistsAsync(string key)
    {
        return await _db.KeyExistsAsync(key);
    }
}