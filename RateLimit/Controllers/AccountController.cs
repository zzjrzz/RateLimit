using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace RateLimit.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly ILogger<AccountController> _logger;
        private readonly IMemoryCache _cache;

        public AccountController(ILogger<AccountController> logger, IMemoryCache cache)
        {
            _logger = logger;
            _cache = cache;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public object Get()
        {
            _cache.CreateEntry("throttle");

            var count = (int?)_cache.Get("throttle") ?? 0;

            count++;

            _cache.Set("throttle", count);

            _logger.LogDebug($"Request made to /api/account {count} times");

            if (count > 10) return StatusCode(429);

            return Ok(count);
        }
    }
}