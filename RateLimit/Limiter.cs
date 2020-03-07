using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using RateLimit.Options;

namespace RateLimit
{
    public class Limiter
    {
        private readonly string _key;
        private readonly IOptionsMonitor<RateLimitOptions> _rateLimitOptions;

        private static readonly ConcurrentDictionary<string, RequestCounter> RequestCache =
            new ConcurrentDictionary<string, RequestCounter>();

        public Limiter(string key, IOptionsMonitor<RateLimitOptions> rateLimitOptions)
        {
            _key = key;
            _rateLimitOptions = rateLimitOptions;
        }

        public bool ShouldLimitRequest()
        {
            var requestCounter = RequestCache.ContainsKey(_key) ? RequestCache[_key] : null;

            if (requestCounter == null || requestCounter.ExpiresOn <= DateTime.Now)
            {
                requestCounter = new RequestCounter
                {
                    ExpiresOn = DateTime.Now.Add(_rateLimitOptions.CurrentValue.Interval),
                    Count = 0
                };
            }

            requestCounter.Count++;

            RequestCache[_key] = requestCounter;

            return (requestCounter.Count > _rateLimitOptions.CurrentValue.MaximumTries);
        }
    }
}