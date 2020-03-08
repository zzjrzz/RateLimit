using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using RateLimit.Options;

namespace RateLimit
{
    public class Limiter
    {
        private readonly IOptionsMonitor<RateLimitOptions> _rateLimitOptions;

        private static readonly ConcurrentDictionary<string, RequestCounter> RequestCache =
            new ConcurrentDictionary<string, RequestCounter>();

        public Limiter(IOptionsMonitor<RateLimitOptions> rateLimitOptions)
        {
            _rateLimitOptions = rateLimitOptions;
        }

        public bool ShouldLimitRequest(string key)
        {
            var requestCounter = GetOrCreateRequestCounter(key);

            requestCounter.Count++;
            RequestCache[key] = requestCounter;

            return (requestCounter.Count > _rateLimitOptions.CurrentValue.MaximumTries);
        }

        public TimeSpan TryAgainTime(string key)
        {
            var requestCounter = GetOrCreateRequestCounter(key);
            return (requestCounter.ExpiresOn - DateTime.Now);
        }

        public RequestCounter GetOrCreateRequestCounter(string key)
        {
            var requestCounter = RequestCache.ContainsKey(key) ? RequestCache[key] : null;

            if (requestCounter == null || requestCounter.ExpiresOn <= DateTime.Now)
            {
                requestCounter = new RequestCounter
                {
                    ExpiresOn = DateTime.Now.Add(_rateLimitOptions.CurrentValue.Interval),
                    Count = 0
                };
            }

            return requestCounter;
        }
    }
}