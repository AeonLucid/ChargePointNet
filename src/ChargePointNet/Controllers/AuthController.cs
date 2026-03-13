using ChargePointNet.Models;
using ChargePointNet.Services.Auth;
using Microsoft.AspNetCore.Mvc;

namespace ChargePointNet.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    ///     Get pending requests
    /// </summary>
    [HttpGet("pending")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<AuthPendingList> GetChargers()
    {
        return Ok(new AuthPendingList
        {
            Results = _authService.GetPending().Select(x => new AuthPending
            {
                RequestId = x.Id,
                Serial = x.Key.Serial,
                CardNumber = x.Key.CardNumber,
                CreatedAt = x.CreatedAt,
                ExpiresAt = x.ExpiresAt,
            })
        });
    }

    /// <summary>
    ///     Approve request
    /// </summary>
    [HttpPost("{requestId:guid}/approve")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<AuthPendingList> ApproveRequest([FromRoute(Name = "requestId")] Guid requestId)
    {
        if (_authService.Authorize(requestId, true))
        {
            return NoContent();
        }
        
        return NotFound();
    }

    /// <summary>
    ///     Reject request
    /// </summary>
    [HttpPost("{requestId:guid}/reject")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<AuthPendingList> RejectRequest([FromRoute(Name = "requestId")] Guid requestId)
    {
        if (_authService.Authorize(requestId, false))
        {
            return NoContent();
        }
        
        return NotFound();
    }
}