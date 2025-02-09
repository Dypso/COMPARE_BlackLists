using StackExchange.Redis;
using Microsoft.Extensions.Caching.Memory;

namespace BlacklistApi.Services;

public interface ITwoLevelCache
{
    Task<bool> GetAsync(string key);
    Task SetAsync(string key, bool value);
}

public class TwoLevelCache : ITwoLevelCache, IDisposable
{
    private readonly IMemoryCache _localCache;
    private readonly IConnectionMultiplexer _redis;
    private readonly ISubscriber _subscriber;
    private readonly string _channelName = "cache:invalidate";
    private readonly TimeSpan _localCacheExpiry = TimeSpan.FromMinutes(5);
    private readonly SemaphoreSlim _lock = new(1, 1);

    public TwoLevelCache(IConnectionMultiplexer redis, IMemoryCache memoryCache)
    {
        _localCache = memoryCache;
        _redis = redis;
        _subscriber = _redis.GetSubscriber();
        
        // Subscribe to cache invalidation events
        _subscriber.Subscribe(_channelName, async (channel, message) => {
            var key = message.ToString();
            await InvalidateLocalCacheAsync(key);
        });
    }

    public async Task<bool> GetAsync(string key)
    {
        // Try get from local cache first
        if (_localCache.TryGetValue(key, out bool localValue))
        {
            return localValue;
        }

        // Get from Redis
        await _lock.WaitAsync();
        try
        {
            // Double-check local cache
            if (_localCache.TryGetValue(key, out localValue))
            {
                return localValue;
            }

            var db = _redis.GetDatabase();
            var redisValue = await db.StringGetAsync(key);
            
            if (redisValue.HasValue)
            {
                var value = bool.Parse(redisValue.ToString());
                _localCache.Set(key, value, _localCacheExpiry);
                return value;
            }

            return false;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task SetAsync(string key, bool value)
    {
        var db = _redis.GetDatabase();
        
        // Update Redis
        await db.StringSetAsync(key, value.ToString());
        
        // Update local cache
        _localCache.Set(key, value, _localCacheExpiry);
        
        // Notify other instances
        await _subscriber.PublishAsync(_channelName, key);
    }

    private async Task InvalidateLocalCacheAsync(string key)
    {
        await _lock.WaitAsync();
        try
        {
            _localCache.Remove(key);
        }
        finally
        {
            _lock.Release();
        }
    }

    public void Dispose()
    {
        _subscriber?.UnsubscribeAll();
        _lock?.Dispose();
    }
}