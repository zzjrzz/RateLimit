using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using RateLimit.Controllers;
using Xunit;

namespace RateLimitTests.Unit
{
    public class AccountControllerUnitTests
    {
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
            var logger = new Mock<ILogger<AccountController>>();
            var memoryCache = GetMemoryCache();
            var controller = new AccountController(logger.Object, memoryCache);

            var response = controller.Get();

            Assert.IsType<OkObjectResult>(response);
            Assert.Equal(1, memoryCache.Get("requestCount"));
        }
    }
}