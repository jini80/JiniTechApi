using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JiniTechApi.Helpers
{
    public class SwaggerFileOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Check if any parameter or property involves IFormFile
            var hasFile = context.MethodInfo
                .GetParameters()
                .Any(p =>
                    typeof(IFormFile).IsAssignableFrom(p.ParameterType) ||
                    (p.ParameterType.GetProperties().Any(prop => typeof(IFormFile).IsAssignableFrom(prop.PropertyType)))
                );

            if (!hasFile) return;

            // Define schema for file uploads
            var schemaProps = new Dictionary<string, OpenApiSchema>
            {
                ["file"] = new OpenApiSchema { Type = "string", Format = "binary" },
                ["prompt"] = new OpenApiSchema { Type = "string" }
            };

            // Optional parameters based on models
            if (context.MethodInfo.Name.Contains("Image", StringComparison.OrdinalIgnoreCase))
            {
                schemaProps["width"] = new OpenApiSchema { Type = "integer", Format = "int32" };
                schemaProps["height"] = new OpenApiSchema { Type = "integer", Format = "int32" };
            }

            if (context.MethodInfo.Name.Contains("Video", StringComparison.OrdinalIgnoreCase))
            {
                schemaProps["voiceBase64"] = new OpenApiSchema { Type = "string" };
            }

            operation.RequestBody = new OpenApiRequestBody
            {
                Content =
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = schemaProps,
                            Required = new HashSet<string> { "prompt" }
                        }
                    }
                }
            };
        }
    }
}
