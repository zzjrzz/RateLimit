using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using RateLimit.Options;

namespace RateLimit
{
    public class Limiter : ILimitingStrategy
    {
        private readonly IOptionsMonitor<RateLimitOptions> _rateLimitOptions;
        private readonly IMemoryCache _cache;

        public Limiter(IOptionsMonitor<RateLimitOptions> rateLimitOptions,
            IMemoryCache cache)
        {
            _rateLimitOptions = rateLimitOptions;
            _cache = cache;
        }

        public bool ShouldLimitRequest(string key)
        {
            var requestCounter = GetOrCreateRequestCounter(key);

            requestCounter.Count++;
            _cache.Set(key, requestCounter);

            return (requestCounter.Count > _rateLimitOptions.CurrentValue.MaximumTries);
        }

        public TimeSpan TryAgainTime(string key)
        {
            var requestCounter = GetOrCreateRequestCounter(key);
            return (requestCounter.ExpiresOn - DateTime.Now);
        }

        public RequestCounter GetOrCreateRequestCounter(string key)
        {
            var requestCounter = (RequestCounter) _cache.Get(key);

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