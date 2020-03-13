using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using RateLimit.Middleware;
using RateLimit.Models.KeyBuilder;
using RateLimit.Models.Limiters;
using RateLimit.Options;
using Xunit;

namespace RateLimitTests.Unit
{
    public class RateLimitingMiddlewareUnitTests
    {
        [Fact]
        public async void Rate_Limiting_Is_Activated_For_Endpoint()
        {
            var optionsMock = new Mock<IOptions<RateLimitOptions>>();
            var rateLimitOption = new RateLimitOptions
            {
                Endpoints = new List<string> {"/api/account"},
                MaximumTries = 1,
                Interval = TimeSpan.FromHours(1)
            };
            optionsMock.Setup(options => options.Value).Returns(rateLimitOption);
            var keyBuilderStrategyMock = new Mock<IKeyBuilderStrategy>();
            var limitingStrategyMock = new Mock<ILimitingStrategy>();

            var middleware = new RateLimitingMiddleware((innerHttpContext) => Task.FromResult(0));
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Path = "/api/account";

            await middleware.InvokeAsync(httpContext, optionsMock.Object,
                keyBuilderStrategyMock.Object, limitingStrategyMock.Object);

            limitingStrategyMock.Verify(limitingStrategy => limitingStrategy.IncrementCount(It.IsAny<string>()));
            limitingStrategyMock.Verify(limitingStrategy => limitingStrategy.ShouldLimitRequest(It.IsAny<string>()));
            keyBuilderStrategyMock.Verify(keyBuilder => keyBuilder.Build(httpContext));
            Assert.Equal(200, httpContext.Response.StatusCode);
        }

        [Fact]
        public async void Rate_Limiting_Is_Returning_429_When_Limit_Exceeded_For_Endpoint()
        {
            var optionsMock = new Mock<IOptions<RateLimitOptions>>();
            var keyBuilderStrategyMock = new Mock<IKeyBuilderStrategy>();
            var limitingStrategyMock = new Mock<ILimitingStrategy>();
            var rateLimitOption = new RateLimitOptions
            {
                Endpoints = new List<string> { "/api/account" },
                MaximumTries = 1,
                Interval = TimeSpan.FromHours(1)
            };
            optionsMock.Setup(options => options.Value).Returns(rateLimitOption);
            limitingStrategyMock.Setup(limitingStrategy => limitingStrategy.ShouldLimitRequest(It.IsAny<string>()))
                .Returns(true);

            var middleware = new RateLimitingMiddleware((innerHttpContext) => Task.FromResult(0));
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Path = "/api/account";

            await middleware.InvokeAsync(httpContext, optionsMock.Object,
                keyBuilderStrategyMock.Object, limitingStrategyMock.Object);

            Assert.Equal(429, httpContext.Response.StatusCode);
        }
    }
}