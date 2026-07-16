using Microsoft.AspNetCore.RateLimiting;
using System.Net;
using System.Threading.RateLimiting;

namespace Tools.AspNet.Security
{
    public static class ServiceCollectionExtensions
    {
        extension(IServiceCollection services)
        {
            public IServiceCollection AddAdvancedSecurity(Action<AdvancedSecurityOptionsBuilder>? builder)
            {
                AdvancedSecurityOptionsBuilder optionsBuilder = new AdvancedSecurityOptionsBuilder();

                if(builder is not null)
                {
                    builder(optionsBuilder);
                }

                services.Add(new ServiceDescriptor(typeof(AdvancedSecurityOptionsBuilder), sp => optionsBuilder, ServiceLifetime.Singleton));

                if(optionsBuilder.UseCors)
                {
                    CorsOptionsBuilder corsOptionsBuilder = optionsBuilder.CorsOptionsBuilder;

                    services.AddCors(o => o.AddPolicy(AdvancedSecurityOptionsBuilder.CorsPolicyName, pb =>
                    {
                        if (corsOptionsBuilder.Origins.Length > 0)
                        {
                            pb.WithOrigins(corsOptionsBuilder.Origins);
                        }

                        //ajout des méthodes
                        List<string> methods = new List<string>() { "OPTIONS" };
                        foreach (string name in Enum.GetNames(typeof(Methods)))
                        {
                            if (optionsBuilder.CorsOptionsBuilder.AllowedMethods.HasFlag((Methods)Enum.Parse(typeof(Methods), name)))
                                methods.Add(name.ToUpper());
                        }

                        pb.WithMethods(methods.ToArray());

                        if(corsOptionsBuilder.Headers.Length > 0)
                        {
                            pb.WithHeaders(corsOptionsBuilder.Headers.Select(h => h.Name).ToArray());
                        }
                    }));
                }

                if (optionsBuilder.UseRateLimit)
                {
                    RateLimitOptionsBuilder rateLimitOptions = optionsBuilder.RateLimitOptionsBuilder;

                    services.AddRateLimiter(options =>
                    {
                        options.GlobalLimiter = PartitionedRateLimiter.CreateChained(
                            PartitionedRateLimiter.Create<HttpContext, string>(httpContext => RateLimitPartition.GetFixedWindowLimiter(partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown", factory: _ => new FixedWindowRateLimiterOptions {
                                PermitLimit = rateLimitOptions.RateLimitPerMinute,
                                Window = TimeSpan.FromMinutes(1)
                            })),
                            PartitionedRateLimiter.Create<HttpContext, string>(httpContext => RateLimitPartition.GetFixedWindowLimiter(partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown", factory: _ => new FixedWindowRateLimiterOptions
                            {
                                PermitLimit = rateLimitOptions.RateLimitPerHour,
                                Window = TimeSpan.FromHours(1)
                            }))
                        );

                        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                        //ou
                        //options.OnRejected = async (context, cancellationToken) =>
                        //{
                        //    // Custom rejection handling logic
                        //    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                        //    context.HttpContext.Response.Headers["Retry-After"] = "60";

                        //    await context.HttpContext.Response.WriteAsync("Rate limit exceeded. Please try again later.", cancellationToken);
                        //};
                    });
                }

                return services;
            }
        }

        extension(IApplicationBuilder app)
        {
            public IApplicationBuilder UseAdvancedSecurity()
            {
                AdvancedSecurityOptionsBuilder advancedSecurityOptionsBuilder = app.ApplicationServices.GetRequiredService<AdvancedSecurityOptionsBuilder>();

                if (advancedSecurityOptionsBuilder.UseCors)
                {
                    app.UseCors(AdvancedSecurityOptionsBuilder.CorsPolicyName);
                }

                if(advancedSecurityOptionsBuilder.UseRateLimit)
                {
                    app.UseRateLimiter();
                }

                app.UseMiddleware<AdvancedSecurityMiddleware>();

                return app;
            }
        }
    }
}
