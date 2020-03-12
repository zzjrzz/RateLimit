using System.Net;
using Microsoft.AspNetCore.Http;
using Moq;
using RateLimit.Models.KeyBuilder;
using Xunit;

namespace RateLimitTests.Unit
{
    public class IpKeyBuilderUnitTests
    {
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;

        public IpKeyBuilderUnitTests()
        {
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        }

        [Fact]
        public void Should_Build_Key_For_Ip()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Method = "GET";
            httpContext.Request.Path = "/api/account";
            httpContext.Connection.RemoteIpAddress = IPAddress.Parse("1.1.1.1");
            _httpContextAccessorMock.Setup(httpContextAccessor => httpContextAccessor.HttpContext)
                .Returns(httpContext);

            var keyBuilder = new IpKeyBuilder(_httpContextAccessorMock.Object);
            Assert.Equal("1.1.1.1_/api/account_get", keyBuilder.Build());
        }
    }
}