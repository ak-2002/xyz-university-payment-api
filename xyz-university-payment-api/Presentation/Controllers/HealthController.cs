using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

namespace xyz_university_payment_api.Presentation.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly HealthCheckService _healthCheckService;
        private readonly ILogger<HealthController> _logger;

        public HealthController(HealthCheckService healthCheckService, ILogger<HealthController> logger)
        {
            _healthCheckService = healthCheckService;
            _logger = logger;
        }

        /// <summary>
        /// Basic health check endpoint
        /// </summary>
        /// <returns>Health status</returns>
        [HttpGet]
        public IActionResult Get()
        {
            _logger.LogInformation("Health check requested");
            
            return Ok(new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Service = "XYZ University Payment API",
                Version = "1.0.0",
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"
            });
        }

        /// <summary>
        /// Detailed health check with all registered health checks
        /// </summary>
        /// <returns>Detailed health status</returns>
        [HttpGet("detailed")]
        public async Task<IActionResult> GetDetailed()
        {
            _logger.LogInformation("Detailed health check requested");
            
            var healthReport = await _healthCheckService.CheckHealthAsync();
            
            var response = new
            {
                Status = healthReport.Status.ToString(),
                Timestamp = DateTime.UtcNow,
                Service = "XYZ University Payment API",
                Version = "1.0.0",
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development",
                Checks = healthReport.Entries.Select(entry => new
                {
                    Name = entry.Key,
                    Status = entry.Value.Status.ToString(),
                    Description = entry.Value.Description,
                    Duration = entry.Value.Duration.TotalMilliseconds
                })
            };

            return healthReport.Status == HealthStatus.Healthy ? Ok(response) : StatusCode(503, response);
        }

        /// <summary>
        /// Readiness probe for Kubernetes
        /// </summary>
        /// <returns>Readiness status</returns>
        [HttpGet("ready")]
        public async Task<IActionResult> GetReady()
        {
            _logger.LogInformation("Readiness check requested");
            
            var healthReport = await _healthCheckService.CheckHealthAsync(registration => registration.Tags.Contains("ready"));
            
            if (healthReport.Status == HealthStatus.Healthy)
            {
                return Ok(new { Status = "Ready", Timestamp = DateTime.UtcNow });
            }
            
            return StatusCode(503, new { Status = "Not Ready", Timestamp = DateTime.UtcNow });
        }

        /// <summary>
        /// Liveness probe for Kubernetes
        /// </summary>
        /// <returns>Liveness status</returns>
        [HttpGet("live")]
        public IActionResult GetLive()
        {
            _logger.LogInformation("Liveness check requested");
            
            return Ok(new { Status = "Alive", Timestamp = DateTime.UtcNow });
        }
    }
} 