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
                    app.UseMiddleware<AdvancedSecurityMiddleware>();
                }

                return app;
            }
        }
    }
}
