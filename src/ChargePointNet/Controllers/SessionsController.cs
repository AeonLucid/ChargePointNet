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
    ///     Get all sessions
    /// </summary>
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<SessionList> GetSessions()
    {
        var sessions = _sessionService.GetAll();
        
        return Ok(new SessionList
        {
            Results = sessions.OrderByDescending(x => x.CreatedAt).Select(y => new Session(y))
        });
    }

    /// <summary>
    ///     Get a single session
    /// </summary>
    /// <param name="sessionId"></param>
    [HttpGet("{sessionId:guid}")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<Session> GetSession([FromRoute(Name = "sessionId")] Guid sessionId)
    {
        var session = _sessionService.Find(sessionId);
        if (session == null)
        {
            return NotFound();
        }

        return Ok(new Session(session));
    }
}