namespace RateLimit.Models.KeyBuilder
{
    public interface IKeyBuilderStrategy
    {
        string Build();
    }
}