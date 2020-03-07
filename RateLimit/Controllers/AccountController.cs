using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RateLimit.Options;

namespace RateLimit.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly ILogger<AccountController> _logger;
        private readonly IOptionsMonitor<RateLimitOptions> _options;
        private readonly IMemoryCache _cache;

        public AccountController(ILogger<AccountController> logger, IMemoryCache cache,
            IOptionsMonitor<RateLimitOptions> options)
        {
            _logger = logger;
            _cache = cache;
            _options = options;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public ObjectResult Get()
        {
            var requestCounter = (RequestCounter) _cache.Get("requestCount") ?? new RequestCounter
            {
                Count = 0,
                ExpiresOn = DateTime.Now.Add(_options.CurrentValue.Interval)
            };

            requestCounter.Count++;

            _cache.Set("requestCount", requestCounter, requestCounter.ExpiresOn);

            _logger.LogDebug($"Request made to /api/account {requestCounter.Count} times");

            return requestCounter.Count > _options.CurrentValue.MaximumTries ? StatusCode(429, "Too many requests") : Ok(requestCounter);
        }
    }
}