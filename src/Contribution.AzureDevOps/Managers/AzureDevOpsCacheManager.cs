using Microsoft.Extensions.Caching.Memory;
using System.Collections.ObjectModel;

namespace Contribution.AzureDevOps.Managers;

public sealed class AzureDevOpsCacheManager(IMemoryCache cache) : IAzureDevOpsCacheManager
{
    private readonly IMemoryCache _cache = cache;

    public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T?>> factory, TimeSpan expiration) where T : class
    {
        if (_cache.TryGetValue<T>(key, out var cached))
            return cached;

        var result = await factory();
        if (result != null)
        {
            _cache.Set(key, result, expiration);
        }

        return result;
    }

    public async Task<IReadOnlyCollection<T>> GetOrSetCollectionAsync<T>(string key, Func<Task<IEnumerable<T>>> factory, TimeSpan expiration)
    {
        if (_cache.TryGetValue(key, out List<T>? cached))
            return new ReadOnlyCollection<T>(cached!);

        var result = await factory();
        var resultList = result.ToList();
        _cache.Set(key, resultList, expiration);
        
        return new ReadOnlyCollection<T>(resultList);
    }

    public bool TryGetValue<T>(string key, out T? value) where T : class
    {
        return _cache.TryGetValue<T>(key, out value);
    }

    public void Set<T>(string key, T value, TimeSpan expiration) where T : class
    {
        _cache.Set(key, value, expiration);
    }
}