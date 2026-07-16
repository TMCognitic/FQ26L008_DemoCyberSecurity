namespace Tools.AspNet.Security
{
    public class AdvancedSecurityOptionsBuilder
    {
        internal const string CorsPolicyName = "Corsican_Policy_Department";

        internal bool UseRateLimit { get; set; } = false;
        internal RateLimitOptionsBuilder RateLimitOptionsBuilder { get; } = new RateLimitOptionsBuilder();

        internal bool UseCors { get; set; } = false;
        internal CorsOptionsBuilder CorsOptionsBuilder { get; } = new CorsOptionsBuilder(); 

        public void EnableCors(Action<CorsOptionsBuilder>? builder = null)
        {
            UseCors = true;

            if (builder is not null)
                builder(CorsOptionsBuilder);
        }

        public void EnableRate(Action<RateLimitOptionsBuilder>? builder = null)
        {
            UseRateLimit = true;

            if (builder is not null)
                builder(RateLimitOptionsBuilder);
        }
    }
}