using System;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RateLimit.Controllers;
using RateLimit.Models;
using RateLimit.Models.KeyBuilder;
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
        }

        private IMemoryCache GetMemoryCache()
        {
            var services = new ServiceCollection();
            services.AddMemoryCache();
            var serviceProvider = services.BuildServiceProvider();
            var memoryCache = serviceProvider.GetService<IMemoryCache>();
            return memoryCache;
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
            var limiter = new SimpleLimiter(_rateLimitOptionsMock.Object, GetMemoryCache());
            var keyBuilder = new IpKeyBuilder(_httpContextAccessorMock.Object);
            var controller = new AccountController(_loggerMock.Object, limiter, keyBuilder);

            var response = controller.Get();

            Assert.Equal(200, response.StatusCode);
        }

        [Fact]
        public void Given_The_Counter_Is_More_Than_Limit_When_Get_Is_Called_Return_429_Response()
        {
            var rateLimitOptions = new RateLimitOptions
            {
                MaximumTries = 100,
                Interval = TimeSpan.FromHours(1)
            };
            _rateLimitOptionsMock.Setup(options => options.CurrentValue).Returns(rateLimitOptions);
            _httpContextAccessorMock.Setup(httpContextAccessor => httpContextAccessor.HttpContext)
                .Returns(new DefaultHttpContext());
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

        [Fact]
        public void Given_Two_Different_Clients_The_Rate_Limiting_Is_Unique_And_Only_One_Exceeds()
        {
            var rateLimitOptions = new RateLimitOptions
            {
                MaximumTries = 1,
                Interval = TimeSpan.FromHours(1)
            };
            _rateLimitOptionsMock.Setup(options => options.CurrentValue).Returns(rateLimitOptions);
            var limiter = new SimpleLimiter(_rateLimitOptionsMock.Object, GetMemoryCache());

            var exceededContext = new DefaultHttpContext();
            exceededContext.Connection.RemoteIpAddress = IPAddress.Parse("2.2.2.2");
            _httpContextAccessorMock.Setup(httpContextAccessor => httpContextAccessor.HttpContext)
                .Returns(exceededContext);
            var exceededKeyBuilder = new IpKeyBuilder(_httpContextAccessorMock.Object);
            var exceededController = new AccountController(_loggerMock.Object, limiter, exceededKeyBuilder);
            exceededController.Get();
            var exceededResponse = exceededController.Get();

            var normalContext = new DefaultHttpContext();
            _httpContextAccessorMock.Setup(httpContextAccessor => httpContextAccessor.HttpContext)
                .Returns(normalContext);
            var keyBuilder = new IpKeyBuilder(_httpContextAccessorMock.Object);
            var controller = new AccountController(_loggerMock.Object, limiter, keyBuilder);
            var response = controller.Get();

            Assert.Equal(200, response.StatusCode);
            Assert.Equal(429, exceededResponse.StatusCode);
        }
    }
}