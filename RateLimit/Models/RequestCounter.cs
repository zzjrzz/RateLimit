using System;

namespace RateLimit.Models
{
    public class RequestCounter
    {
        public DateTime ExpiresOn { get; set; }
        public int Count { get; set; }
    }
}