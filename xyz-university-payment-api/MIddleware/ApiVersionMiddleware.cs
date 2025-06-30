using xyz_university_payment_api.Services;

namespace xyz_university_payment_api.Middleware
{
    /// <summary>
    /// Middleware for handling API version deprecation warnings and version negotiation
    /// </summary>
    public class ApiVersionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ApiVersionService _apiVersionService;
        private readonly ILogger<ApiVersionMiddleware> _logger;

        public ApiVersionMiddleware(
            RequestDelegate next,
            ApiVersionService apiVersionService,
            ILogger<ApiVersionMiddleware> logger)
        {
            _next = next;
            _apiVersionService = apiVersionService;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Extract API version from the request path
            var version = ExtractApiVersion(context.Request.Path);
            
            if (!string.IsNullOrEmpty(version))
            {
                // Log API version usage
                _logger.LogInformation("API version {Version} requested for {Path}", version, context.Request.Path);

                // Check if version is supported
                if (!_apiVersionService.IsVersionSupported(version))
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        Error = "Unsupported API version",
                        Message = $"API version {version} is not supported",
                        SupportedVersions = _apiVersionService.GetAllVersions().Select(v => v.Version),
                        DefaultVersion = _apiVersionService.GetDefaultVersion()
                    });
                    return;
                }

                // Check for deprecation warnings
                if (_apiVersionService.ShouldShowDeprecationWarning(version))
                {
                    var deprecationWarning = _apiVersionService.GetDeprecationWarning(version);
                    if (deprecationWarning != null)
                    {
                        // Add deprecation warning headers
                        context.Response.Headers.Append("X-API-Version-Deprecated", "true");
                        context.Response.Headers.Append("X-API-Version-Sunset-Date", deprecationWarning.SunsetDate?.ToString("yyyy-MM-dd"));
                        context.Response.Headers.Append("X-API-Version-Migration-Guide", deprecationWarning.MigrationGuide);
                        
                        _logger.LogWarning("Deprecated API version {Version} used. Sunset date: {SunsetDate}", 
                            version, deprecationWarning.SunsetDate);
                    }
                }

                // Add version information to response headers
                context.Response.Headers.Append("X-API-Version", version);
                context.Response.Headers.Append("X-API-Version-Status", 
                    _apiVersionService.GetVersionInfo(version)?.Status ?? "Unknown");
            }

            await _next(context);
        }

        private string? ExtractApiVersion(PathString path)
        {
            // Extract version from path like /api/v1/payments or /api/v2/students
            var segments = path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments?.Length >= 2 && segments[0] == "api" && segments[1].StartsWith("v"))
            {
                return segments[1].Substring(1); // Remove 'v' prefix
            }
            return null;
        }
    }

    /// <summary>
    /// Extension method for registering the API version middleware
    /// </summary>
    public static class ApiVersionMiddlewareExtensions
    {
        public static IApplicationBuilder UseApiVersioning(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ApiVersionMiddleware>();
        }
    }
} 