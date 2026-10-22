using Microsoft.AspNetCore.Mvc;

namespace DocsAi.Api.Features.Chat;

[ApiController]
[Route("chat")]
public class ChatController(ChatService chatService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ChatReply>> Post(ChatRequest request, CancellationToken cancellationToken)
    {
        var reply = await chatService.SendAsync(request.Message, cancellationToken);
        return Ok(reply);
    }
}
