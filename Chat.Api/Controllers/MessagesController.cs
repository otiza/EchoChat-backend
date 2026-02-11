using Chat.Application.Common.Contracts;
using Chat.Application.Messages.Contracts;
using Chat.Application.Messages.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chat.Api.Controllers;

[ApiController]
[Route("conversations/{conversationId}/messages")]
[Authorize]
public sealed class MessagesController : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<MessageResponse>> Send(
        [FromRoute] string conversationId,
        [FromBody] SendMessageRequest request,
        [FromServices] SendMessageService service,
        CancellationToken ct)
    {
        var msg = await service.SendAsync(conversationId, request.Content, ct);
        return Ok(msg);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MessageResponse>>> Get(
        [FromRoute] string conversationId,
        [FromQuery] DateTime? before,
        [FromQuery] int limit,
        [FromServices] GetMessagesService service,
        CancellationToken ct)
    {
        var page = new PagedRequest(before, limit);
        var list = await service.GetAsync(conversationId, page, ct);
        return Ok(list);
    }
}