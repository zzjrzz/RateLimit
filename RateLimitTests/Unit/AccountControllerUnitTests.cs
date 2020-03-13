using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using RateLimit.Controllers;
using RateLimit.Options;
using Xunit;

namespace RateLimitTests.Unit
{
    public class AccountControllerUnitTests
    {
        private readonly Mock<IOptionsMonitor<RateLimitOptions>> _rateLimitOptionsMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;

        public AccountControllerUnitTests()
        {
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _rateLimitOptionsMock = new Mock<IOptionsMonitor<RateLimitOptions>>();
        }
        
        [Fact]
        public void Given_The_Counter_Is_Less_Than_Limit_Ensure_The_Response_200()
        {
            var rateLimitOptions = new RateLimitOptions
            {
                MaximumTries = 1,
                Interval = TimeSpan.FromHours(1)
            };
            _rateLimitOptionsMock.Setup(options => options.CurrentValue).Returns(rateLimitOptions);
            _httpContextAccessorMock.Setup(httpContextAccessor => httpContextAccessor.HttpContext)
                .Returns(new DefaultHttpContext());
            var controller = new AccountController();

            var response = controller.Get();

            Assert.Equal(200, response.StatusCode);
        }
    }
}