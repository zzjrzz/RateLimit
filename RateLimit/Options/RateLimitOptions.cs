using System;

namespace RateLimit.Options
{
    public class RateLimitOptions
    {
        public int MaximumTries { get; set; }
        public TimeSpan Interval { get; set; }
    }
}