using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
                MaximumTries = 1
            };
            _rateLimitOptionsMock.Setup(options => options.CurrentValue).Returns(rateLimitValues);
            var memoryCache = GetMemoryCache();
            var controller = new AccountController(_loggerMock.Object, memoryCache, _rateLimitOptionsMock.Object);

            var response = controller.Get();

            Assert.IsType<OkObjectResult>(response);
            Assert.Equal(200, response.StatusCode);
            Assert.Equal(1, memoryCache.Get("requestCount"));
        }

        [Fact]
        public void Given_The_Counter_Is_More_Than_10_When_Get_Is_Called_Return_429_Response()
        {
            var rateLimitValues = new RateLimitOptions
            {
                MaximumTries = 10
            };
            _rateLimitOptionsMock.Setup(options => options.CurrentValue).Returns(rateLimitValues);
            var memoryCache = GetMemoryCache();
            memoryCache.Set("requestCount", 100);
            var controller = new AccountController(_loggerMock.Object, memoryCache, _rateLimitOptionsMock.Object);

            var response = controller.Get();

            Assert.IsType<ObjectResult>(response);
            Assert.Equal(429, response.StatusCode);
            Assert.Equal(101, memoryCache.Get("requestCount"));
        }
    }
}