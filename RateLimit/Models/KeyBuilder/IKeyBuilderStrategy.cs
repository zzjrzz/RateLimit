using Microsoft.AspNetCore.Http;

namespace RateLimit.Models.KeyBuilder
{
    public interface IKeyBuilderStrategy
    {
        string Build(HttpContext context);
    }
}