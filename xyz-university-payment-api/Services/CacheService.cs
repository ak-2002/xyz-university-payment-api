using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System.Text.Json;
using xyz_university_payment_api.Interfaces;
using xyz_university_payment_api.Models;

namespace xyz_university_payment_api.Services
{
    /// <summary>
    /// Redis cache service implementation
    /// </summary>
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly RedisConfig _redisConfig;
        private readonly ILogger<CacheService> _logger;

        public CacheService(IDistributedCache cache, IOptions<RedisConfig> redisConfig, ILogger<CacheService> logger)
        {
            _cache = cache;
            _redisConfig = redisConfig.Value;
            _logger = logger;
        }

        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            try
            {
                var cachedValue = await _cache.GetStringAsync(key);
                if (string.IsNullOrEmpty(cachedValue))
                {
                    _logger.LogDebug("Cache miss for key: {Key}", key);
                    return null;
                }

                _logger.LogDebug("Cache hit for key: {Key}", key);
                return JsonSerializer.Deserialize<T>(cachedValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving from cache for key: {Key}", key);
                return null;
            }
        }

        public async Task SetAsync<T>(string key, T value) where T : class
        {
            await SetAsync(key, value, TimeSpan.FromMinutes(_redisConfig.DefaultExpirationMinutes));
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan expiration) where T : class
        {
            try
            {
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration
                };

                var serializedValue = JsonSerializer.Serialize(value);
                await _cache.SetStringAsync(key, serializedValue, options);
                
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
                // Note: This is a simplified implementation
                // In a production environment, you might want to use Redis SCAN command
                // or implement a more sophisticated pattern matching
                _logger.LogWarning("Pattern-based cache removal not fully implemented for pattern: {Pattern}", pattern);
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