
namespace FortunaeLibraryManagementSystem.Service.Services.CacheService;
public interface IRedisService
{
    Task<T?> GetAsync<T>(string key);
    Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task<bool> RemoveAsync(string key);
    Task<bool> KeyExistsAsync(string key);
}