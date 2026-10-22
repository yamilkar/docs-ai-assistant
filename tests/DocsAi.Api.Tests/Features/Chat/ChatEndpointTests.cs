using System.Net;
using System.Net.Http.Json;
using DocsAi.Api.Features.Chat;
using DocsAi.Api.Tests.Common;
using Microsoft.Extensions.AI;

namespace DocsAi.Api.Tests.Features.Chat;

[Collection(ApiCollection.Name)]
public class ChatEndpointTests(ApiFixture api)
{
    private readonly HttpClient _client = api.Client;

    [Fact]
    public async Task Post_Chat_ReturnsModelReply()
    {
        var response = await _client.PostAsJsonAsync("/chat", new ChatRequest("¿Qué es la glucosa?"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ChatReply>();
        Assert.Equal("echo: ¿Qué es la glucosa?", body?.Reply);
    }

    [Fact]
    public async Task Post_Chat_SendsSystemPromptBeforeUserMessage()
    {
        await _client.PostAsJsonAsync("/chat", new ChatRequest("Hola"));

        var messages = api.ChatClient.LastMessages;
        Assert.Equal(ChatRole.System, messages[0].Role);
        Assert.Contains("nunca das diagnósticos", messages[0].Text);
        Assert.Equal(ChatRole.User, messages[^1].Role);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Post_Chat_WithoutMessage_ReturnsBadRequest(string? message)
    {
        var response = await _client.PostAsJsonAsync("/chat", new { message });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
