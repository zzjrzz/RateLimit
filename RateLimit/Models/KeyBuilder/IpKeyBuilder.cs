using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace RateLimit.Models.KeyBuilder
{
    public class IpKeyBuilder : IKeyBuilderStrategy
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public IpKeyBuilder(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public RequestIdentifier CreateRequestIdentifier()
        {
            return new RequestIdentifier
            {
                IpAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString(),
                Path = _httpContextAccessor.HttpContext.Request.Path.ToString().ToLowerInvariant(),
                HttpMethod = _httpContextAccessor.HttpContext.Request.Method.ToLowerInvariant()
            };
        }

        public string Build()
        {
            var bytes = Encoding.UTF8.GetBytes(CreateRequestIdentifier().ToString());
            using var algorithm = new SHA1Managed();
            var hash = algorithm.ComputeHash(bytes);

            return Convert.ToBase64String(hash);
        }
    }
}