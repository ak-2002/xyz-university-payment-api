using Microsoft.AspNetCore.Mvc;
using xyz_university_payment_api.Core.Application.Services;
using xyz_university_payment_api.Core.Shared.Constants;
using xyz_university_payment_api.Core.Application.DTOs;

namespace xyz_university_payment_api.Presentation.Controllers
{
    [ApiController]
    [Route("api/versions")]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [ApiVersion("3.0")]
    public class ApiVersionController : ControllerBase
    {
        private readonly ApiVersionService _apiVersionService;
        private readonly ILogger<ApiVersionController> _logger;

        public ApiVersionController(ApiVersionService apiVersionService, ILogger<ApiVersionController> logger)
        {
            _apiVersionService = apiVersionService;
            _logger = logger;
        }

        /// <summary>
        /// Get information about all supported API versions
        /// </summary>
        [HttpGet]
        public IActionResult GetAllVersions()
        {
            var versions = _apiVersionService.GetAllVersions();
            
            return Ok(new ApiResponse<IEnumerable<ApiVersionInfo>>
            {
                Success = true,
                Message = "API versions retrieved successfully",
                Data = versions,
                Metadata = new Dictionary<string, object>
                {
                    ["DefaultVersion"] = _apiVersionService.GetDefaultVersion(),
                    ["LatestVersion"] = _apiVersionService.GetLatestVersion(),
                    ["TotalVersions"] = versions.Count()
                }
            });
        }

        /// <summary>
        /// Get information about a specific API version
        /// </summary>
        [HttpGet("{version}")]
        public IActionResult GetVersionInfo(string version)
        {
            var versionInfo = _apiVersionService.GetVersionInfo(version);
            
            if (versionInfo == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"API version {version} not found",
                    Errors = new List<string> { "Version not supported" }
                });
            }

            var response = new ApiResponse<ApiVersionInfo>
            {
                Success = true,
                Message = $"API version {version} information retrieved",
                Data = versionInfo
            };

            // Add deprecation warning if applicable
            if (_apiVersionService.IsVersionDeprecated(version))
            {
                var deprecationWarning = _apiVersionService.GetDeprecationWarning(version);
                response.Metadata = new Dictionary<string, object>
                {
                    ["DeprecationWarning"] = deprecationWarning,
                    ["IsDeprecated"] = true,
                    ["RecommendedVersion"] = _apiVersionService.GetLatestVersion()
                };
            }

            return Ok(response);
        }

        /// <summary>
        /// Get deprecation warnings for all deprecated versions
        /// </summary>
        [HttpGet("deprecation-warnings")]
        public IActionResult GetDeprecationWarnings()
        {
            var warnings = new List<ApiVersionDeprecationWarning>();
            
            foreach (var version in _apiVersionService.GetAllVersions())
            {
                if (_apiVersionService.IsVersionDeprecated(version.Version))
                {
                    var warning = _apiVersionService.GetDeprecationWarning(version.Version);
                    if (warning != null)
                    {
                        warnings.Add(warning);
                    }
                }
            }

            return Ok(new ApiResponse<IEnumerable<ApiVersionDeprecationWarning>>
            {
                Success = true,
                Message = "Deprecation warnings retrieved successfully",
                Data = warnings,
                Metadata = new Dictionary<string, object>
                {
                    ["TotalWarnings"] = warnings.Count,
                    ["RecommendedVersion"] = _apiVersionService.GetLatestVersion()
                }
            });
        }

        /// <summary>
        /// Compare two API versions
        /// </summary>
        [HttpGet("compare")]
        public IActionResult CompareVersions([FromQuery] string fromVersion, [FromQuery] string toVersion)
        {
            if (string.IsNullOrEmpty(fromVersion) || string.IsNullOrEmpty(toVersion))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Both fromVersion and toVersion parameters are required",
                    Errors = new List<string> { "Missing required parameters" }
                });
            }

            var comparison = _apiVersionService.GetVersionComparison(fromVersion, toVersion);
            
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = $"Version comparison from {fromVersion} to {toVersion}",
                Data = comparison
            });
        }

        /// <summary>
        /// Get migration guide for upgrading from one version to another
        /// </summary>
        [HttpGet("migration-guide")]
        public IActionResult GetMigrationGuide([FromQuery] string fromVersion, [FromQuery] string toVersion)
        {
            if (string.IsNullOrEmpty(fromVersion) || string.IsNullOrEmpty(toVersion))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Both fromVersion and toVersion parameters are required",
                    Errors = new List<string> { "Missing required parameters" }
                });
            }

            var fromInfo = _apiVersionService.GetVersionInfo(fromVersion);
            var toInfo = _apiVersionService.GetVersionInfo(toVersion);

            if (fromInfo == null || toInfo == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "One or both versions not found",
                    Errors = new List<string> { "Version not supported" }
                });
            }

            var migrationGuide = new
            {
                FromVersion = fromInfo,
                ToVersion = toInfo,
                BreakingChanges = toInfo.BreakingChanges,
                NewFeatures = toInfo.Features,
                MigrationSteps = GenerateMigrationSteps(fromVersion, toVersion),
                DocumentationUrl = toInfo.DocumentationUrl,
                MigrationRequired = fromInfo.Status == "Deprecated" || fromInfo.Status == "Sunset"
            };

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = $"Migration guide from {fromVersion} to {toVersion}",
                Data = migrationGuide
            });
        }

        /// <summary>
        /// Get the current recommended API version
        /// </summary>
        [HttpGet("recommended")]
        public IActionResult GetRecommendedVersion()
        {
            var recommendedVersion = _apiVersionService.GetLatestVersion();
            var versionInfo = _apiVersionService.GetVersionInfo(recommendedVersion);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Recommended API version retrieved",
                Data = new
                {
                    RecommendedVersion = recommendedVersion,
                    VersionInfo = versionInfo ?? new ApiVersionInfo(),
                    Reason = "Latest stable version with all features"
                }
            });
        }

        private List<string> GenerateMigrationSteps(string fromVersion, string toVersion)
        {
            var steps = new List<string>();

            if (fromVersion == "1.0" && toVersion == "2.0")
            {
                steps.AddRange(new[]
                {
                    "Update API base URL from /api/v1/ to /api/v2/",
                    "Update response handling to include metadata field",
                    "Implement new validation rules for enhanced security",
                    "Update error handling to use new error response format",
                    "Review breaking changes in response structure",
                    "Test all endpoints with new version",
                    "Update client libraries to V2 compatible versions"
                });
            }

            return steps;
        }
    }
} 