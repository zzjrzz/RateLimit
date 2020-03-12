using Microsoft.AspNetCore.Http;
using Moq;
using RateLimit.Models;
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
            _httpContextAccessorMock.Setup(httpContextAccessor => httpContextAccessor.HttpContext)
                .Returns(httpContext);

            var keyBuilder = new IpKeyBuilder(_httpContextAccessorMock.Object);
            Assert.Equal("__", keyBuilder.Build());
        }
    }
}