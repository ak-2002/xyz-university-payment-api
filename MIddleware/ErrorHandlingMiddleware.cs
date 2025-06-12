using System.Net;
using System.Text.Json;

namespace xyz_university_payment_api.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly  RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger=logger;
        }
        public async Task InvokeAsync(HttpContext context){
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred.");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "apllication/Json";

                var errorResponse = new
                {
                    message = "An unexpected error occurred. Please try again later."
                };

                var errorJson = JsonSerializer.Serialize(errorResponse);
                await context.Response.WriteAsync(errorJson);
                
                throw;
            }
        }
    }
}