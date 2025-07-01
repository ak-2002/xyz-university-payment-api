using Microsoft.AspNetCore.Mvc;
using xyz_university_payment_api.Interfaces;
using xyz_university_payment_api.DTOs;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace xyz_university_payment_api.Controllers
{
    /// <summary>
    /// Cache management controller for testing and monitoring Redis cache
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [ApiVersion("3.0")]
    public class CacheController : ControllerBase
    {
        private readonly ICacheService _cacheService;
        private readonly ILogger<CacheController> _logger;

        public CacheController(ICacheService cacheService, ILogger<CacheController> logger)
        {
            _cacheService = cacheService;
            _logger = logger;
        }

        /// <summary>
        /// Test cache functionality by setting and retrieving a value
        /// </summary>
        [HttpPost("test")]
        public async Task<ActionResult<ApiResponse<string>>> TestCache([FromBody] CacheTestRequest request)
        {
            try
            {
                var testKey = $"test:{request.Key}";
                
                // Set value in cache
                await _cacheService.SetAsync(testKey, request.Value, TimeSpan.FromMinutes(5));
                _logger.LogInformation("Test value cached: {Key}", testKey);

                // Retrieve value from cache
                var cachedValue = await _cacheService.GetAsync<string>(testKey);
                
                if (cachedValue == request.Value)
                {
                    return Ok(new ApiResponse<string>
                    {
                        Success = true,
                        Message = "Cache test successful",
                        Data = cachedValue
                    });
                }
                else
                {
                    return BadRequest(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Cache test failed - retrieved value doesn't match",
                        Data = cachedValue
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing cache");
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Cache test failed",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Check if a key exists in cache
        /// </summary>
        [HttpGet("exists/{key}")]
        public async Task<ActionResult<ApiResponse<bool>>> CheckCacheExists(string key)
        {
            try
            {
                var exists = await _cacheService.ExistsAsync(key);
                
                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = exists ? "Key exists in cache" : "Key not found in cache",
                    Data = exists
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking cache existence for key: {Key}", key);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Error checking cache existence",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Remove a key from cache
        /// </summary>
        [HttpDelete("remove/{key}")]
        public async Task<ActionResult<ApiResponse<string>>> RemoveFromCache(string key)
        {
            try
            {
                await _cacheService.RemoveAsync(key);
                _logger.LogInformation("Removed key from cache: {Key}", key);
                
                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = "Key removed from cache successfully",
                    Data = key
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing key from cache: {Key}", key);
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Error removing key from cache",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Get cache statistics
        /// </summary>
        [HttpGet("stats")]
        public ActionResult<ApiResponse<CacheStats>> GetCacheStats()
        {
            try
            {
                var stats = new CacheStats
                {
                    Timestamp = DateTime.UtcNow,
                    Message = "Cache statistics retrieved successfully",
                    Note = "Redis statistics are available through Redis CLI or monitoring tools"
                };

                return Ok(new ApiResponse<CacheStats>
                {
                    Success = true,
                    Message = "Cache statistics retrieved",
                    Data = stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cache statistics");
                return StatusCode(500, new ApiResponse<CacheStats>
                {
                    Success = false,
                    Message = "Error getting cache statistics",
                    Errors = new List<string> { ex.Message }
                });
            }
        }
    }

    /// <summary>
    /// Request model for cache testing
    /// </summary>
    public class CacheTestRequest
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    /// <summary>
    /// Cache statistics model
    /// </summary>
    public class CacheStats
    {
        public DateTime Timestamp { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
    }
}