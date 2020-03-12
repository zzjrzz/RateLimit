﻿using Microsoft.AspNetCore.Http;

namespace RateLimit.Models.KeyBuilder
{
    public class IpKeyBuilder : IKeyBuilderStrategy
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public IpKeyBuilder(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string Build()
        {
            var requestIdentifier = new RequestIdentifier
            {
                IpAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString(),
                Path = _httpContextAccessor.HttpContext.Request.Path.ToString().ToLowerInvariant(),
                HttpMethod = _httpContextAccessor.HttpContext.Request.Method.ToLowerInvariant()
            };
            return $"{requestIdentifier.IpAddress}_{requestIdentifier.Path}_{requestIdentifier.HttpMethod}";
        }
    }

}