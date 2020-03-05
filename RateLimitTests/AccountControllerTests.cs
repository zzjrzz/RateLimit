using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using RateLimit;
using Xunit;

namespace RateLimitTests
{
    public class AccountControllerTests: IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;

        public AccountControllerTests(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Theory]
        [InlineData("/api/account")]
        public async Task Get_EndpointsReturnSuccessAndCorrectContentType(string url)
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync(url);

            response.EnsureSuccessStatusCode();
        }
    }
}
