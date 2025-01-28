using Microsoft.Extensions.Caching.Memory;

namespace Chat.Cache
{
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<CacheService> _logger;

        public CacheService(IMemoryCache cache, ILogger<CacheService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public T Get<T>(string key)
        {
            try
            {
                T value;
                if (_cache.TryGetValue(key, out value))
                {
                    _logger.LogInformation($"Cache hit for key: {key}");
                    return value;
                }

                _logger.LogInformation($"Cache miss for key: {key}");
                return default(T);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Cache get error for key {key}: {ex.Message}");
                return default(T);
            }
        }

        public void Set<T>(string key, T value, int durationInMinutes = 60)
        {
            try
            {
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(durationInMinutes))
                    .SetAbsoluteExpiration(TimeSpan.FromHours(24))
                    .SetPriority(CacheItemPriority.Normal)
                    .SetSize(1024);

                _cache.Set(key, value, cacheEntryOptions);
                _logger.LogInformation($"Cache set for key: {key}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Cache set error for key {key}: {ex.Message}");
            }
        }

        public void Remove(string key)
        {
            try
            {
                _cache.Remove(key);
                _logger.LogInformation($"Cache removed for key: {key}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Cache remove error for key {key}: {ex.Message}");
            }
        }

        public bool Exists(string key)
        {
            return _cache.TryGetValue(key, out _);
        }

        public void Clear()
        {
            try
            {
                if (_cache is MemoryCache memoryCache)
                {
                    memoryCache.Compact(1.0);
                    _logger.LogInformation("Cache cleared");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Cache clear error: {ex.Message}");
            }
        }
    }
}
