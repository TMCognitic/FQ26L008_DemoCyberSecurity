namespace Tools.AspNet.Security
{
    public class AdvancedSecurityOptionsBuilder
    {
        internal CorsOptionsBuilder CorsOptionsBuilder { get; } = new CorsOptionsBuilder(); 

        internal bool UseCors { get; set; } = false;
        internal const string CorsPolicyName = "Corsican_Policy_Department";

        public void EnableCors(Action<CorsOptionsBuilder>? builder = null)
        {
            UseCors = true;

            if (builder is not null)
                builder(CorsOptionsBuilder);
        }
    }
}