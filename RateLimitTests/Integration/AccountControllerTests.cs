using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using RateLimit;
using Xunit;

namespace RateLimitTests.Integration
{
    public class AccountControllerTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;

        public AccountControllerTests(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Get_Endpoint_Is_Ok_Until_Exceeding_Limit_Returns_Too_Many_Requests()
        {
            var client = _factory.CreateClient();

            for (var times = 1; times <= 100; times++)
            {
                var response = await client.GetAsync("/api/account");
                response.EnsureSuccessStatusCode();
            }

            var lastResponse = await client.GetAsync("/api/account");
            Assert.Equal(HttpStatusCode.TooManyRequests, lastResponse.StatusCode);
        }
    }
}