using Microsoft.Extensions.AI;

namespace DocsAi.Api.Tests.Common;

/// <summary>
/// Stands in for Claude on Bedrock so tests never call AWS. Echoes the last user message
/// and keeps the messages it received so tests can check what the API sent.
/// </summary>
public sealed class FakeChatClient : IChatClient
{
    public IReadOnlyList<ChatMessage> LastMessages { get; private set; } = [];

    public Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        LastMessages = messages.ToList();
        var userText = LastMessages.Last(m => m.Role == ChatRole.User).Text;
        return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, $"echo: {userText}")));
    }

    public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public object? GetService(Type serviceType, object? serviceKey = null) => null;

    public void Dispose()
    {
    }
}
