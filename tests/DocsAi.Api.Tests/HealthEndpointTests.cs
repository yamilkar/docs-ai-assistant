using System.Net;
using System.Net.Http.Json;
using DocsAi.Api.Tests.Common;

namespace DocsAi.Api.Tests;

[Collection(ApiCollection.Name)]
public class HealthEndpointTests(ApiFixture api)
{
    private readonly HttpClient _client = api.Client;

    [Fact]
    public async Task Get_Health_ReturnsOkWithHealthyStatus()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<HealthResponse>();
        Assert.Equal("healthy", body?.Status);
    }
}
