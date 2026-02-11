using Chat.Application.Conversations.Contracts;
using Chat.Application.Conversations.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chat.Api.Controllers;

[ApiController]
[Route("conversations")]
[Authorize]
public sealed class ConversationsController : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ConversationSummaryResponse>> Create(
        [FromBody] CreateConversationRequest request,
        [FromServices] CreateConversationService service,
        CancellationToken ct)
    {
        var created = await service.CreateAsync(request.ParticipantUserIds, ct);
        return Ok(created);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ConversationSummaryResponse>>> GetMine(
        [FromQuery] int limit,
        [FromServices] GetMyConversationsService service,
        CancellationToken ct)
    {
        var list = await service.GetAsync(limit, ct);
        return Ok(list);
    }
}