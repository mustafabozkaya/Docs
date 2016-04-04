using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.TestHost;
using TestingControllersSample;
using TestingControllersSample.ClientModels;
using Xunit;

namespace TestingControllerSample.Tests.IntegrationTests
{
    public class ApiIdeasControllerForSession
    {
        private readonly TestServer _server;
        private readonly HttpClient _client;

        public ApiIdeasControllerForSession()
        {
            _server = new TestServer(TestServer.CreateBuilder()
                .UseEnvironment("Development")
                .UseStartup<Startup>());
            _client = _server.CreateClient();

            // client always expects json results
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        [Fact]
        public async Task ReturnsNotFoundForBadSessionId()
        {
            var response = await _client.GetAsync("/api/ideas/forsession/500");
            Assert.Equal(HttpStatusCode.NotFound,response.StatusCode);
        }

        [Fact]
        public async Task ReturnsIdeasForValidSessionId()
        {
            var response = await _client.GetAsync("/api/ideas/forsession/1");
            response.EnsureSuccessStatusCode();

            var ideaList = await response.Content.ReadAsJsonAsync<List<IdeaDTO>>();
            var firstIdea = ideaList.First();
            var testSession = Startup.GetTestSession();
            Assert.Equal(testSession.Ideas.First().Name, firstIdea.name);
        }
    }
}