using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;

namespace RateLimit.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly ILogger<AccountController> _logger;
        private readonly ILimitingStrategy _limiter;

        public AccountController(ILogger<AccountController> logger, ILimitingStrategy limiter)
        {
            _logger = logger;
            _limiter = limiter;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public IStatusCodeActionResult Get()
        {
            if (!_limiter.ShouldLimitRequest("requestCounter")) return StatusCode(200);

            _logger.LogWarning($"Request to /api/account was limited");
            return StatusCode(429,
                $"Rate limit exceeded. Try again in {(int) _limiter.TryAgainTime("requestCounter").TotalSeconds} seconds.");
        }
    }
}