using JiniTechApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace JiniTechApi.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiKeyAuthAttribute : Attribute, IAsyncActionFilter
    {
        private const string ApiKeyHeaderName = ApiKeyService.HeaderName;

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var hasHeader = context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey);
            if (!hasHeader)
            {
                context.Result = new UnauthorizedObjectResult(new { error = $"API Key missing in header '{ApiKeyHeaderName}'." });
                return;
            }

            var svc = context.HttpContext.RequestServices.GetService<ApiKeyService>();
            if (svc == null || !svc.ValidateApiKey(extractedApiKey.ToString()))
            {
                context.Result = new UnauthorizedObjectResult(new { error = "Invalid or revoked API Key." });
                return;
            }

            await next();
        }
    }
}
