using System;

namespace RateLimit
{
    public interface ILimitingStrategy
    {
        bool ShouldLimitRequest(string key);
        TimeSpan TryAgainTime(string key);
    }
}
