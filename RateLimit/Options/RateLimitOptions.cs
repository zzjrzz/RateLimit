using System;

namespace RateLimit.Options
{
    public class RateLimitOptions
    {
        public RateLimitOptions()
        {
        }

        public int MaximumTries { get; set; }
        public TimeSpan Interval { get; set; }
    }
}