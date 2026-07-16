using Microsoft.Extensions.Options;
using System.Net;

namespace Tools.AspNet.Security
{
    public class AdvancedSecurityMiddleware(RequestDelegate next, 
        ILogger<AdvancedSecurityMiddleware> logger, 
        AdvancedSecurityOptionsBuilder advancedSecurityOptions)
    {
        public async Task InvokeAsync(HttpContext httpContext)
        {
            string path = httpContext.Request.Path.Value ?? "/";
            string method = httpContext.Request.Method;
            IPAddress? address = httpContext.Connection.RemoteIpAddress;

            if (!await ValidateHeaders(httpContext))
            {
                logger.LogWarning("Request blocked from {Address} by personal header verification for {Method} {Path}", address?.ToString() ?? "none", method, path);
                return; // on n'appelle PAS _next() -> la pipeline s'arrête ici
            }

            // Pas d'erreur, on continue
            await next(httpContext);
        }

        private async Task<bool> ValidateHeaders(HttpContext httpContext)
        {
            CorsOptionsBuilder corsOptions = advancedSecurityOptions.CorsOptionsBuilder;

            if (corsOptions.Headers.Length > 0)
            {
                foreach (HeaderInfo header in corsOptions.Headers)
                {
                    if (!httpContext.Request.Headers.TryGetValue(header.Name, out var headerValue) || string.IsNullOrWhiteSpace(headerValue))
                    {
                        logger.LogError($"Requête rejetée : header '{header.Name}' manquant.");

                        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                        httpContext.Response.ContentType = "application/json";
                        await httpContext.Response.WriteAsJsonAsync(new
                        {
                            error = "Configuration des headers invalide."
                        });
                        return false;
                    }
                    else if (header.Validator is not null && !header.Validator(headerValue!))
                    {
                        logger.LogError($"Requête rejetée : la validation du header '{header.Name}' a échouée.");

                        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                        httpContext.Response.ContentType = "application/json";
                        await httpContext.Response.WriteAsJsonAsync(new
                        {
                            error = "Configuration des headers invalide."
                        });
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
