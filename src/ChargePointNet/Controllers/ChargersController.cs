using ChargePointNet.Core;
using ChargePointNet.Models;
using ChargePointNet.Models.Requests;
using Microsoft.AspNetCore.Mvc;

namespace ChargePointNet.Controllers;

[ApiController]
[Route("api/chargers")]
public class ChargersController : ControllerBase
{
    private readonly EVManager _evManager;

    public ChargersController(EVManager evManager)
    {
        _evManager = evManager;
    }

    /// <summary>
    ///     Get all chargers
    /// </summary>
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<ChargerList> GetChargers()
    {
        return Ok(new ChargerList
        {
            Results = _evManager.ChargeBoxes
                .Where(x => x.Initialized)
                .Select(x => new Charger(x))
        });
    }
    
    /// <summary>
    ///     Get a single charger
    /// </summary>
    /// <param name="serial">The serial of a charger.</param>
    [HttpGet("{serial}")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<Charger> GetCharger([FromRoute(Name = "serial")] string serial)
    {
        var charger = _evManager.FindBySerial(serial);
        if (charger == null)
        {
            return NotFound();
        }

        return Ok(new Charger(charger));
    }
    
    /// <summary>
    ///     Update autostart
    /// </summary>
    /// <param name="serial">The serial of a charger.</param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("{serial}/autostart")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult UpdateAutostart([FromRoute(Name = "serial")] string serial, [FromBody] AutostartUpdateRequest request)
    {
        var charger = _evManager.FindBySerial(serial);
        if (charger == null)
        {
            return NotFound();
        }
        
        charger.UpdateAutostart(request.Enabled);
        
        return Ok();
    }
    
    /// <summary>
    ///     Update led brightness
    /// </summary>
    /// <param name="serial">The serial of a charger.</param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("{serial}/led-brightness")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult UpdateLedBrightness([FromRoute(Name = "serial")] string serial, [FromBody] LedBrightnessUpdateRequest request)
    {
        var charger = _evManager.FindBySerial(serial);
        if (charger == null)
        {
            return NotFound();
        }
        
        charger.UpdateLedBrightness(request.Brightness);
        
        return Ok();
    }
}