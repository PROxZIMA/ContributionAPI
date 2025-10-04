using Contribution.Common.Models;

namespace Contribution.Common.Managers;

public interface ICacheManager
{
    Task<T?> GetOrSetAsync<T>(string key, Func<Task<T?>> factory, TimeSpan expiration) where T : class;
    Task<CacheResult<T>> GetOrSetWithStatusAsync<T>(string key, Func<Task<T?>> factory, TimeSpan expiration) where T : class;
    Task<IReadOnlyCollection<T>> GetOrSetCollectionAsync<T>(string key, Func<Task<IEnumerable<T>>> factory, TimeSpan expiration);
    bool TryGetValue<T>(string key, out T? value) where T : class;
    void Set<T>(string key, T value, TimeSpan expiration) where T : class;
}