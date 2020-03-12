using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RateLimit.Controllers;
using RateLimit.Models;
using RateLimit.Models.Limiters;
using RateLimit.Options;
using Xunit;

namespace RateLimitTests.Unit
{
    public class AccountControllerUnitTests
    {
        private readonly Mock<IOptionsMonitor<RateLimitOptions>> _rateLimitOptionsMock;
        private readonly Mock<ILogger<AccountController>> _loggerMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;

        public AccountControllerUnitTests()
        {
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _rateLimitOptionsMock = new Mock<IOptionsMonitor<RateLimitOptions>>();
            _loggerMock = new Mock<ILogger<AccountController>>();

            _httpContextAccessorMock.Setup(httpContextAccessor => httpContextAccessor.HttpContext)
                .Returns(new DefaultHttpContext());
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
                Interval = TimeSpan.FromHours(1)
            };
            _rateLimitOptionsMock.Setup(options => options.CurrentValue).Returns(rateLimitValues);
            var limiter = new SimpleLimiter(_rateLimitOptionsMock.Object, GetMemoryCache());
            var keyBuilder = new IpKeyBuilder(_httpContextAccessorMock.Object);
            var controller = new AccountController(_loggerMock.Object, limiter, keyBuilder);

            var response = controller.Get();

            Assert.Equal(200, response.StatusCode);
        }

        [Fact]
        public void Given_The_Counter_Is_More_Than_100_When_Get_Is_Called_Return_429_Response()
        {
            var rateLimitValues = new RateLimitOptions
            {
                MaximumTries = 100,
                Interval = TimeSpan.FromHours(1)
            };
            _rateLimitOptionsMock.Setup(options => options.CurrentValue).Returns(rateLimitValues);
            var keyBuilder = new IpKeyBuilder(_httpContextAccessorMock.Object);
            var cache = GetMemoryCache();
            cache.Set(keyBuilder.Build(), new RequestCounter
            {
                Count = 100,
                ExpiresOn = DateTime.MaxValue
            }, DateTime.MaxValue);
            var limiter = new SimpleLimiter(_rateLimitOptionsMock.Object, cache);

            var controller = new AccountController(_loggerMock.Object, limiter, keyBuilder);

            var response = controller.Get();

            Assert.Equal(429, response.StatusCode);
        }
    }
}