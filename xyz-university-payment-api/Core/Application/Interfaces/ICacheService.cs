namespace xyz_university_payment_api.Core.Application.Interfaces
{
    /// <summary>
    /// Cache service interface for Redis operations
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Get value from cache
        /// </summary>
        Task<T?> GetAsync<T>(string key);

        /// <summary>
        /// Set value in cache with default expiration
        /// </summary>
        Task SetAsync<T>(string key, T value);

        /// <summary>
        /// Set value in cache with custom expiration
        /// </summary>
        Task SetAsync<T>(string key, T value, TimeSpan expiration);

        /// <summary>
        /// Remove value from cache
        /// </summary>
        Task RemoveAsync(string key);

        /// <summary>
        /// Remove multiple keys from cache
        /// </summary>
        Task RemoveByPatternAsync(string pattern);

        /// <summary>
        /// Check if key exists in cache
        /// </summary>
        Task<bool> ExistsAsync(string key);

        /// <summary>
        /// Clear all cache
        /// </summary>
        Task ClearAllCacheAsync();

        /// <summary>
        /// Get all tracked keys
        /// </summary>
        List<string> GetAllTrackedKeys();

        /// <summary>
        /// Get cache key with prefix
        /// </summary>
        string GetCacheKey(string prefix, string identifier);

        /// <summary>
        /// Get cache key for payments
        /// </summary>
        string GetPaymentCacheKey(string identifier);

        /// <summary>
        /// Get cache key for students
        /// </summary>
        string GetStudentCacheKey(string identifier);

        /// <summary>
        /// Get cache key for summaries
        /// </summary>
        string GetSummaryCacheKey(string identifier);
    }
}