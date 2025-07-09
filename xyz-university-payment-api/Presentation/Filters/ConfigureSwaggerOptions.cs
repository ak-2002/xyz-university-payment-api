using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace xyz_university_payment_api.Presentation.Filters
{
    public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _provider;

        public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
        {
            _provider = provider;
        }

        public void Configure(SwaggerGenOptions options)
        {
            // Add a swagger document for each discovered API version
            foreach (var description in _provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description));
            }
        }

        private static OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
        {
            var info = new OpenApiInfo
            {
                Title = "XYZ University Payment API",
                Version = description.ApiVersion.ToString(),
                Description = GetDescriptionForVersion(description.ApiVersion.MajorVersion ?? 0),
                Contact = new OpenApiContact
                {
                    Name = "XYZ University API Support",
                    Email = "api-support@xyz-university.com"
                }
            };

            if (description.IsDeprecated)
            {
                info.Description += " This API version has been deprecated.";
            }

            return info;
        }

        private static string GetDescriptionForVersion(int version)
        {
            return version switch
            {
                1 => "V1 API Endpoints (Deprecated - Use V2 or V3)",
                2 => "V2 API Endpoints (Recommended)",
                3 => "V3 API Endpoints (Latest Features)",
                _ => "API Endpoints"
            };
        }
    }
} 