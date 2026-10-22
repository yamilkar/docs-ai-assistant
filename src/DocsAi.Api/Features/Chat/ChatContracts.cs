using System.ComponentModel.DataAnnotations;

namespace DocsAi.Api.Features.Chat;

public record ChatRequest([Required, StringLength(4000, MinimumLength = 1)] string Message);

// Named ChatReply, not ChatResponse, to avoid clashing with Microsoft.Extensions.AI.ChatResponse.
public record ChatReply(string Reply);
