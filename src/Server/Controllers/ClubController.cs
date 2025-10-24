using Microsoft.AspNetCore.Mvc;
using NetAppForVika.Server.Abstractions;
using NetAppForVika.Shared.Dtos;

namespace NetAppForVika.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ClubController : ControllerBase
{
    private readonly IClubSessionCoordinator _coordinator;

    public ClubController(IClubSessionCoordinator coordinator)
    {
        _coordinator = coordinator;
    }

    [HttpGet("{sessionId}")]
    [ProducesResponseType(typeof(ClubSessionStateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSession(string sessionId, CancellationToken cancellationToken)
    {
        try
        {
            var state = await _coordinator.GetSessionAsync(sessionId, cancellationToken);
            return Ok(state);
        }
        catch (Exception ex)
        {
            return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
