using System;

namespace RateLimit
{
    public class RequestCounter
    {
        public DateTime ExpiresOn { get; set; }
        public int Count { get; set; }
    }
}