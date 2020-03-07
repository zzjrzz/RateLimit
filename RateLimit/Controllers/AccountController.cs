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

        public AccountController(ILogger<AccountController> logger, IMemoryCache cache, IOptionsMonitor<RateLimitOptions> options)
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
            var count = (int?)_cache.Get("requestCount") ?? 0;

            count++;

            _cache.Set("requestCount", count);

            _logger.LogDebug($"Request made to /api/account {count} times");

            return count > _options.CurrentValue.MaximumTries ? StatusCode(429, "Too many requests") : Ok(count);
        }
    }
}