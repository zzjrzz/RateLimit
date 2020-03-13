using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace RateLimit.Models.KeyBuilder
{
    public class IpKeyBuilder : IKeyBuilderStrategy
    {
        public IpKeyBuilder()
        {
        }

        public RequestIdentifier CreateRequestIdentifier(HttpContext httpContext)
        {
            return new RequestIdentifier
            {
                IpAddress = httpContext.Connection.RemoteIpAddress?.ToString(),
                Path = httpContext.Request.Path.ToString().ToLowerInvariant(),
                HttpMethod = httpContext.Request.Method.ToLowerInvariant()
            };
        }

        public string Build(HttpContext httpContext)
        {
            var bytes = Encoding.UTF8.GetBytes(CreateRequestIdentifier(httpContext).ToString());
            using var algorithm = new SHA1Managed();
            var hash = algorithm.ComputeHash(bytes);

            return Convert.ToBase64String(hash);
        }
    }
}