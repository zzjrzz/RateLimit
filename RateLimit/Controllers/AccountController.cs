using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using RateLimit.Models.KeyBuilder;
using RateLimit.Models.Limiters;

namespace RateLimit.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly ILogger<AccountController> _logger;
        private readonly ILimitingStrategy _limiter;
        private readonly IKeyBuilderStrategy _keyBuilderStrategy;

        public AccountController(ILogger<AccountController> logger, ILimitingStrategy limiter, IKeyBuilderStrategy keyBuilderStrategy)
        {
            _logger = logger;
            _limiter = limiter;
            _keyBuilderStrategy = keyBuilderStrategy;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public IStatusCodeActionResult Get()
        {
            _limiter.IncrementCount(_keyBuilderStrategy.Build());

            if (!_limiter.ShouldLimitRequest(_keyBuilderStrategy.Build())) return StatusCode(200);

            _logger.LogWarning($"Request to /api/account was limited");
            return StatusCode(429,
                $"Rate limit exceeded. Try again in {(int) _limiter.TryAgainTime(_keyBuilderStrategy.Build()).TotalSeconds} seconds.");
        }
    }
}