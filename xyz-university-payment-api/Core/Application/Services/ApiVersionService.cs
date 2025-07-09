using Microsoft.Extensions.Options;
using xyz_university_payment_api.Core.Shared.Constants;

namespace xyz_university_payment_api.Core.Application.Services
{
    /// <summary>
    /// Service for managing API version information and deprecation warnings
    /// </summary>
    public class ApiVersionService
    {
        private readonly ApiVersionConfig _config;
        private readonly ILogger<ApiVersionService> _logger;
        private readonly Dictionary<string, ApiVersionInfo> _versionInfo;

        public ApiVersionService(IOptions<ApiVersionConfig> config, ILogger<ApiVersionService> logger)
        {
            _config = config.Value;
            _logger = logger;
            _versionInfo = InitializeVersionInfo();
        }

        /// <summary>
        /// Get information about a specific API version
        /// </summary>
        public ApiVersionInfo? GetVersionInfo(string version)
        {
            return _versionInfo.TryGetValue(version, out var info) ? info : null;
        }

        /// <summary>
        /// Get all supported API versions
        /// </summary>
        public IEnumerable<ApiVersionInfo> GetAllVersions()
        {
            return _versionInfo.Values;
        }

        /// <summary>
        /// Check if a version is deprecated
        /// </summary>
        public bool IsVersionDeprecated(string version)
        {
            return _config.DeprecatedVersions.Contains(version);
        }

        /// <summary>
        /// Check if a version is supported
        /// </summary>
        public bool IsVersionSupported(string version)
        {
            return _config.SupportedVersions.Contains(version);
        }

        /// <summary>
        /// Get deprecation warning for a version
        /// </summary>
        public ApiVersionDeprecationWarning? GetDeprecationWarning(string version)
        {
            if (!IsVersionDeprecated(version))
                return null;

            var versionInfo = GetVersionInfo(version);
            if (versionInfo == null)
                return null;

            return new ApiVersionDeprecationWarning
            {
                Version = version,
                Message = $"API version {version} is deprecated and will be sunset on {versionInfo.SunsetDate:yyyy-MM-dd}",
                DeprecationDate = versionInfo.DeprecationDate,
                SunsetDate = versionInfo.SunsetDate,
                MigrationGuide = $"https://api.xyz-university.com/docs/migration/{version}-to-{_config.DefaultVersion}"
            };
        }

        /// <summary>
        /// Get the default API version
        /// </summary>
        public string GetDefaultVersion()
        {
            return _config.DefaultVersion;
        }

        /// <summary>
        /// Get the latest API version
        /// </summary>
        public string GetLatestVersion()
        {
            return _config.SupportedVersions
                .Where(v => !IsVersionDeprecated(v))
                .OrderByDescending(v => Version.Parse(v))
                .FirstOrDefault() ?? _config.DefaultVersion;
        }

        /// <summary>
        /// Validate if a version should trigger a deprecation warning
        /// </summary>
        public bool ShouldShowDeprecationWarning(string version)
        {
            if (!_config.ShowDeprecationWarnings || !IsVersionDeprecated(version))
                return false;

            var versionInfo = GetVersionInfo(version);
            if (versionInfo?.SunsetDate == null)
                return false;

            var daysUntilSunset = (versionInfo.SunsetDate.Value - DateTime.UtcNow).Days;
            return daysUntilSunset <= _config.DeprecationWarningDays;
        }

        /// <summary>
        /// Get version comparison information
        /// </summary>
        public object GetVersionComparison(string fromVersion, string toVersion)
        {
            var fromInfo = GetVersionInfo(fromVersion);
            var toInfo = GetVersionInfo(toVersion);

            return new
            {
                FromVersion = fromInfo,
                ToVersion = toInfo,
                BreakingChanges = toInfo?.BreakingChanges ?? Array.Empty<string>(),
                NewFeatures = toInfo?.Features ?? Array.Empty<string>(),
                MigrationRequired = fromInfo?.Status == "Deprecated" || fromInfo?.Status == "Sunset"
            };
        }

        private Dictionary<string, ApiVersionInfo> InitializeVersionInfo()
        {
            return new Dictionary<string, ApiVersionInfo>
            {
                ["1.0"] = new ApiVersionInfo
                {
                    Version = "1.0",
                    Status = "Deprecated",
                    DeprecationDate = DateTime.Parse("2024-01-01"),
                    SunsetDate = DateTime.Parse("2024-12-31"),
                    Features = new[] { "Basic CRUD Operations", "Simple Pagination", "Basic Validation" },
                    BreakingChanges = new[] { "Response format changes in V2", "New required fields in V2" },
                    DocumentationUrl = "https://api.xyz-university.com/docs/v1"
                },
                ["2.0"] = new ApiVersionInfo
                {
                    Version = "2.0",
                    Status = "Current",
                    Features = new[] {
                        "Enhanced Validation",
                        "Improved Error Handling",
                        "Metadata Support",
                        "Analytics Endpoints",
                        "Advanced Filtering",
                        "Real-time Notifications"
                    },
                    BreakingChanges = Array.Empty<string>(),
                    DocumentationUrl = "https://api.xyz-university.com/docs/v2"
                },
                ["3.0"] = new ApiVersionInfo
                {
                    Version = "3.0",
                    Status = "Current",
                    Features = new[] {
                        "Real-time Processing",
                        "AI-powered Validation",
                        "Advanced Analytics",
                        "Webhook Integration",
                        "GraphQL Compatibility",
                        "Microservices Architecture",
                        "Predictive Insights",
                        "Enhanced Bulk Operations"
                    },
                    BreakingChanges = Array.Empty<string>(),
                    DocumentationUrl = "https://api.xyz-university.com/docs/v3"
                }
            };
        }
    }
}