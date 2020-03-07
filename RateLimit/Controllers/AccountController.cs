using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
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

        public AccountController(ILogger<AccountController> logger, IOptionsMonitor<RateLimitOptions> options)
        {
            _logger = logger;
            _options = options;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public IStatusCodeActionResult Get()
        {
            var limiter = new Limiter("requestCode", _options);
            if (!limiter.ShouldLimitRequest()) return StatusCode(200);
            
            _logger.LogDebug($"Request to /api/account was limited");
            return StatusCode(429, "Too many requests");
        }
    }
}