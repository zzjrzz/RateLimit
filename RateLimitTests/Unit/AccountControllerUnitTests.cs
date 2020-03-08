using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RateLimit;
using RateLimit.Controllers;
using RateLimit.Options;
using Xunit;

namespace RateLimitTests.Unit
{
    public class AccountControllerUnitTests
    {
        private readonly Mock<IOptionsMonitor<RateLimitOptions>> _rateLimitOptionsMock;
        private readonly Mock<ILogger<AccountController>> _loggerMock;

        public AccountControllerUnitTests()
        {
            _rateLimitOptionsMock = new Mock<IOptionsMonitor<RateLimitOptions>>();
            _loggerMock = new Mock<ILogger<AccountController>>();
        }

        public IMemoryCache GetMemoryCache()
        {
            var services = new ServiceCollection();
            services.AddMemoryCache();
            var serviceProvider = services.BuildServiceProvider();

            var memoryCache = serviceProvider.GetService<IMemoryCache>();
            return memoryCache;
        }

        [Fact]
        public void Ensure_The_Counter_Goes_Up_By_One_When_Get_Is_Called()
        {
            var rateLimitValues = new RateLimitOptions
            {
                MaximumTries = 1,
                Interval = TimeSpan.FromMinutes(1)
            };
            _rateLimitOptionsMock.Setup(options => options.CurrentValue).Returns(rateLimitValues);
            var limiter = new Limiter( _rateLimitOptionsMock.Object, GetMemoryCache());
            var controller = new AccountController(_loggerMock.Object, limiter);

            var response = controller.Get();

            Assert.Equal(200, response.StatusCode);
        }

        [Fact]
        public void Given_The_Counter_Is_More_Than_10_When_Get_Is_Called_Return_429_Response()
        {
            var rateLimitValues = new RateLimitOptions
            {
                MaximumTries = 0,
                Interval = TimeSpan.FromMinutes(1)
            };
            _rateLimitOptionsMock.Setup(options => options.CurrentValue).Returns(rateLimitValues);
            var cache = GetMemoryCache();
            cache.Set("requestCounter", new RequestCounter
            {
                Count = 10,
                ExpiresOn = DateTime.MaxValue
            });
            var limiter = new Limiter(_rateLimitOptionsMock.Object, GetMemoryCache());
            var controller = new AccountController(_loggerMock.Object, limiter);

            var response = controller.Get();

            Assert.Equal(429, response.StatusCode);
        }
    }
}