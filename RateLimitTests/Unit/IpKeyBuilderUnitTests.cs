using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;
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
        public void Should_Format_Parameters_In_Format_Of_Ip_Path_Http_Method()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Connection.RemoteIpAddress = IPAddress.Parse("1.1.1.1");
            httpContext.Request.Path = "/api/account";
            httpContext.Request.Method = "GET";
            _httpContextAccessorMock.Setup(httpContextAccessor => httpContextAccessor.HttpContext)
                .Returns(httpContext);

            var keyBuilder = new IpKeyBuilder(_httpContextAccessorMock.Object);
            Assert.Equal("1.1.1.1_/api/account_get", keyBuilder.CreateRequestIdentifier().ToString());
        }

        [Fact]
        public void Should_Hash_Identity_Using_SHA1()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Connection.RemoteIpAddress = IPAddress.Parse("1.1.1.1");
            httpContext.Request.Path = "/api/account";
            httpContext.Request.Method = "GET";
            _httpContextAccessorMock.Setup(httpContextAccessor => httpContextAccessor.HttpContext)
                .Returns(httpContext);

            var keyBuilder = new IpKeyBuilder(_httpContextAccessorMock.Object);
            var bytes = Encoding.UTF8.GetBytes(keyBuilder.CreateRequestIdentifier().ToString());
            using var algorithm = new SHA1Managed();
            var hash = algorithm.ComputeHash(bytes);

            Assert.Equal(Convert.ToBase64String(hash), keyBuilder.Build());
        }
    }
}