using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System.Text.Json;
using xyz_university_payment_api.Core.Application.Interfaces;
using xyz_university_payment_api.Infrastructure.External.Caching;

namespace xyz_university_payment_api.Core.Application.Services
{
    /// <summary>
    /// Redis cache service implementation
    /// </summary>
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly RedisConfig _redisConfig;
        private readonly ILogger<CacheService> _logger;
        private readonly HashSet<string> _cacheKeys = new HashSet<string>();

        public CacheService(IDistributedCache cache, IOptions<RedisConfig> redisConfig, ILogger<CacheService> logger)
        {
            _cache = cache;
            _redisConfig = redisConfig.Value;
            _logger = logger;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                var cachedValue = await _cache.GetStringAsync(key);
                if (string.IsNullOrEmpty(cachedValue))
                {
                    _logger.LogDebug("Cache miss for key: {Key}", key);
                    return default;
                }

                _logger.LogDebug("Cache hit for key: {Key}", key);
                return JsonSerializer.Deserialize<T>(cachedValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving from cache for key: {Key}", key);
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value)
        {
            await SetAsync(key, value, TimeSpan.FromMinutes(_redisConfig.DefaultExpirationMinutes));
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan expiration)
        {
            try
            {
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration
                };

                var serializedValue = JsonSerializer.Serialize(value);
                await _cache.SetStringAsync(key, serializedValue, options);

                // Track the cache key for pattern-based removal
                lock (_cacheKeys)
                {
                    _cacheKeys.Add(key);
                }

                _logger.LogDebug("Cached value for key: {Key} with expiration: {Expiration}", key, expiration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache for key: {Key}", key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await _cache.RemoveAsync(key);
                
                // Remove from tracked keys
                lock (_cacheKeys)
                {
                    _cacheKeys.Remove(key);
                }
                
                _logger.LogDebug("Removed cache key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache key: {Key}", key);
            }
        }

        public async Task RemoveByPatternAsync(string pattern)
        {
            try
            {
                var keysToRemove = new List<string>();
                
                lock (_cacheKeys)
                {
                    // Find all keys that match the pattern
                    foreach (var key in _cacheKeys)
                    {
                        // Handle wildcard patterns
                        if (pattern.Contains("*"))
                        {
                            var patternWithoutWildcard = pattern.Replace("*", "");
                            if (key.Contains(patternWithoutWildcard))
                            {
                                keysToRemove.Add(key);
                            }
                        }
                        else
                        {
                            // Exact match or contains match
                            if (key.Contains(pattern))
                            {
                                keysToRemove.Add(key);
                            }
                        }
                    }
                }

                // Remove all matching keys
                foreach (var key in keysToRemove)
                {
                    await RemoveAsync(key);
                }

                _logger.LogInformation("Removed {Count} cache keys matching pattern: {Pattern}", keysToRemove.Count, pattern);
                
                // Log the keys that were removed for debugging
                if (keysToRemove.Count > 0)
                {
                    _logger.LogDebug("Removed keys: {Keys}", string.Join(", ", keysToRemove));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache by pattern: {Pattern}", pattern);
            }
        }

        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                var value = await _cache.GetStringAsync(key);
                return !string.IsNullOrEmpty(value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking cache existence for key: {Key}", key);
                return false;
            }
        }

        public List<string> GetAllTrackedKeys()
        {
            lock (_cacheKeys)
            {
                return _cacheKeys.ToList();
            }
        }

        public async Task ClearAllCacheAsync()
        {
            try
            {
                var keysToRemove = new List<string>();
                
                lock (_cacheKeys)
                {
                    keysToRemove.AddRange(_cacheKeys);
                }

                foreach (var key in keysToRemove)
                {
                    await RemoveAsync(key);
                }

                _logger.LogInformation("Cleared all cache keys. Total removed: {Count}", keysToRemove.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing all cache");
            }
        }

        public string GetCacheKey(string prefix, string identifier)
        {
            return $"{_redisConfig.InstanceName}{prefix}:{identifier}";
        }

        public string GetPaymentCacheKey(string identifier)
        {
            return GetCacheKey("payment", identifier);
        }

        public string GetStudentCacheKey(string identifier)
        {
            return GetCacheKey("student", identifier);
        }

        public string GetSummaryCacheKey(string identifier)
        {
            return GetCacheKey("summary", identifier);
        }
    }
}