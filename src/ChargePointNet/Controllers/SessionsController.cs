using ChargePointNet.Models;
using ChargePointNet.Services.Sessions;
using Microsoft.AspNetCore.Mvc;

namespace ChargePointNet.Controllers;

[ApiController]
[Route("api/sessions")]
public class SessionsController : ControllerBase
{
    private readonly ISessionService _sessionService;
    
    public SessionsController(ISessionService sessionService)
    {
        _sessionService = sessionService;
    }
    
    /// <summary>
    ///     Get charging sessions
    /// </summary>
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<SessionList> GetSessions()
    {
        var sessions = _sessionService.GetAll();
        
        return Ok(new SessionList
        {
            Results = sessions.OrderByDescending(x => x.CreatedAt).Select(y => new Session
            {
                SessionId = y.Id,
                Serial = y.Key.Serial,
                CardNumber = y.Key.CardNumber,
                IsCharging = y.IsCharging,
                MeterValueStart = y.MeterValueStart,
                MeterValueCurrent = y.MeterValueCurrent,
                MeterValueEnd = y.MeterValueEnd,
                CreatedAt = y.CreatedAt,
                UpdatedAt = y.UpdatedAt,
                EndedAt = y.EndedAt,
            })
        });
    }
}