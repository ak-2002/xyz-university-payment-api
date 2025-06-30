using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace xyz_university_payment_api.Filters
{
    /// <summary>
    /// Operation filter for API versioning in Swagger
    /// </summary>
    public class ApiVersionOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Add API version header parameter
            operation.Parameters ??= new List<OpenApiParameter>();
            
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-API-Version",
                In = ParameterLocation.Header,
                Description = "API version to use (e.g., 1.0, 2.0)",
                Required = false,
                Schema = new OpenApiSchema
                {
                    Type = "string",
                    Example = new Microsoft.OpenApi.Any.OpenApiString("2.0")
                }
            });

            // Add deprecation warning for V1 endpoints
            if (context.ApiDescription.RelativePath?.Contains("/v1/") == true)
            {
                operation.Description = $"{operation.Description}\n\n**⚠️ Deprecated:** This endpoint uses API V1 which is deprecated. Please migrate to V2.";
            }
        }
    }
} 