using StackExchange.Redis;
using System.Collections.ObjectModel;
using System.Text.Json;
using Contribution.Common.Models;

namespace Contribution.Common.Managers;

public sealed class CacheManager : ICacheManager, IDisposable
{
    private readonly IConnectionMultiplexer _respConn;
    private readonly IDatabase _db;
    private readonly JsonSerializerOptions _jsonOptions;

    public CacheManager(IConnectionMultiplexer respConn)
    {
        _respConn = respConn;
        _db = _respConn.GetDatabase();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };
    }

    public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T?>> factory, TimeSpan expiration) where T : class
    {
        var cachedValue = await _db.StringGetAsync(key);
        if (cachedValue.HasValue)
        {
            return JsonSerializer.Deserialize<T>(cachedValue!, _jsonOptions);
        }

        var result = await factory();
        if (result != null)
        {
            var serialized = JsonSerializer.Serialize(result, _jsonOptions);
            await _db.StringSetAsync(key, serialized, expiration);
        }

        return result;
    }

    public async Task<CacheResult<T>> GetOrSetWithStatusAsync<T>(string key, Func<Task<T?>> factory, TimeSpan expiration) where T : class
    {
        var cachedValue = await _db.StringGetAsync(key);
        if (cachedValue.HasValue)
        {
            var cached = JsonSerializer.Deserialize<T>(cachedValue!, _jsonOptions);
            return new CacheResult<T>(cached, true); // Cache hit
        }

        var result = await factory();
        if (result != null)
        {
            var serialized = JsonSerializer.Serialize(result, _jsonOptions);
            await _db.StringSetAsync(key, serialized, expiration);
        }

        return new CacheResult<T>(result, false); // Cache miss
    }

    public async Task<IReadOnlyCollection<T>> GetOrSetCollectionAsync<T>(string key, Func<Task<IEnumerable<T>>> factory, TimeSpan expiration)
    {
        var cachedValue = await _db.StringGetAsync(key);
        if (cachedValue.HasValue)
        {
            var cached = JsonSerializer.Deserialize<List<T>>(cachedValue!, _jsonOptions);
            return new ReadOnlyCollection<T>(cached ?? new List<T>());
        }

        var result = await factory();
        var resultList = result.ToList();
        
        var serialized = JsonSerializer.Serialize(resultList, _jsonOptions);
        await _db.StringSetAsync(key, serialized, expiration);
        
        return new ReadOnlyCollection<T>(resultList);
    }

    public bool TryGetValue<T>(string key, out T? value) where T : class
    {
        var cachedValue = _db.StringGet(key);
        if (cachedValue.HasValue)
        {
            value = JsonSerializer.Deserialize<T>(cachedValue!, _jsonOptions);
            return value != null;
        }

        value = null;
        return false;
    }

    public void Set<T>(string key, T value, TimeSpan expiration) where T : class
    {
        var serialized = JsonSerializer.Serialize(value, _jsonOptions);
        _db.StringSet(key, serialized, expiration);
    }

    public void Dispose()
    {
        // ConnectionMultiplexer is managed by the DI container, no explicit disposal needed
    }
}