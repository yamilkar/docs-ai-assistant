using Microsoft.Extensions.AI;

namespace DocsAi.Api.Features.Chat;

public sealed class ChatService(IChatClient chatClient)
{
    internal const string SystemPrompt =
        "Eres el asistente de Mi Salud. Respondes en español, con frases cortas y claras. " +
        "Explicas conceptos médicos generales, pero nunca das diagnósticos ni cambias tratamientos: " +
        "para eso, recomienda hablar con el médico.";

    public async Task<ChatReply> SendAsync(string message, CancellationToken cancellationToken)
    {
        List<ChatMessage> messages =
        [
            new(ChatRole.System, SystemPrompt),
            new(ChatRole.User, message),
        ];

        var response = await chatClient.GetResponseAsync(messages, cancellationToken: cancellationToken);

        return new ChatReply(response.Text);
    }
}
