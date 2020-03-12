using System;

namespace RateLimit.Models.Limiters
{
    public interface ILimitingStrategy
    {
        bool ShouldLimitRequest(string key);
        TimeSpan TryAgainTime(string key);
        void IncrementCount(string key);
    }
}
