namespace RateLimit.Models
{
    public class RequestIdentifier
    {
        public string IpAddress { get; set; }
        public string Path { get; set; }
        public string HttpMethod { get; set; }

        public override string ToString()
        {
            return $"{IpAddress}_{Path}_{HttpMethod}";
        }
    }
}