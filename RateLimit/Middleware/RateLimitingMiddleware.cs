using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using RateLimit.Models.KeyBuilder;
using RateLimit.Models.Limiters;
using RateLimit.Options;

namespace RateLimit.Middleware
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;

        public RateLimitingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IOptions<RateLimitOptions> options, IKeyBuilderStrategy keyBuilderStrategy, ILimitingStrategy limitingStrategy)
        {
            if (options == null)
            {
                await _next.Invoke(context);
                return;
            }

            foreach (var endpoint in options.Value.Endpoints)
            {
                if (string.Equals(context.Request.Path.ToString(), endpoint,
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    limitingStrategy.IncrementCount(keyBuilderStrategy.Build(context));
                    if (limitingStrategy.ShouldLimitRequest(keyBuilderStrategy.Build(context)))
                    {
                        context.Response.StatusCode = 429;
                        await context.Response.WriteAsync(
                            $"Rate limit exceeded. Try again in {(int) limitingStrategy.TryAgainTime(keyBuilderStrategy.Build(context)).TotalSeconds} seconds.");
                        return;
                    }
                }
            }

            await _next(context);
        }
    }
}