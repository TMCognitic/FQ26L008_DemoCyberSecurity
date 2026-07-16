namespace Tools.AspNet.Security
{
    public class RateLimitOptionsBuilder
    {
        public int RateLimitPerMinute { get; set; } = 60;
        public int RateLimitPerHour { get; set; } = 3000;
    }
}
