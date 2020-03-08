using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using RateLimit.Options;

namespace RateLimit
{
    public class SimpleLimiter : ILimitingStrategy
    {
        private readonly IOptionsMonitor<RateLimitOptions> _rateLimitOptions;
        private readonly IMemoryCache _cache;

        public SimpleLimiter(
            IOptionsMonitor<RateLimitOptions> rateLimitOptions,
            IMemoryCache cache)
        {
            _rateLimitOptions = rateLimitOptions;
            _cache = cache;
        }

        public bool ShouldLimitRequest(string key)
        {
            var requestCounter = GetOrCreateRequestCounter(key);

            IncrementCount(key, requestCounter);

            return (requestCounter.Count > _rateLimitOptions.CurrentValue.MaximumTries);
        }

        public void IncrementCount(string key, RequestCounter requestCounter)
        {
            requestCounter.Count++;
            _cache.Set(key, requestCounter, requestCounter.ExpiresOn);
        }

        public TimeSpan TryAgainTime(string key)
        {
            var requestCounter = GetOrCreateRequestCounter(key);
            return (requestCounter.ExpiresOn - DateTime.Now);
        }

        public RequestCounter GetOrCreateRequestCounter(string key)
        {
            if (_cache.TryGetValue(key, out RequestCounter requestCounter) && requestCounter.ExpiresOn > DateTime.Now)
                return requestCounter;

            requestCounter = new RequestCounter
            {
                ExpiresOn = DateTime.Now.Add(_rateLimitOptions.CurrentValue.Interval),
                Count = 0
            };
            _cache.Set(key, requestCounter, requestCounter.ExpiresOn);
            return requestCounter;
        }
    }
}