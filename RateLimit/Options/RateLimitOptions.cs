using System;
using System.Collections.Generic;

namespace RateLimit.Options
{
    public class RateLimitOptions
    {
        public List<string> Endpoints { get; set; }
        public int MaximumTries { get; set; }
        public TimeSpan Interval { get; set; }
    }
}