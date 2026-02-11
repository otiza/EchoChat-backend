using System.Security.Claims;
using Chat.Application.Contracts;
using Chat.Application.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chat.Api.Controllers;

[ApiController]
[Route("users")]
public sealed class UsersController : ControllerBase
{
    private readonly GetMeService _me;
    private readonly SearchUsersService _search;

    public UsersController(GetMeService me, SearchUsersService search)
    {
        _me = me;
        _search = search;
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserMeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var userId =
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub"); // our token uses JwtRegisteredClaimNames.Sub

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized(new { error = "Missing user id claim." });
        
        var result = await _me.GetMeAsync(userId, ct);
        return Ok(result);


    }
    
    [Authorize]
    [HttpGet("search")]
    [ProducesResponseType(typeof(IReadOnlyList<UserSearchResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] int limit = 10, CancellationToken ct = default)
    {
        var results = await _search.SearchAsync(q, limit, ct);
        return Ok(results);
    }
}