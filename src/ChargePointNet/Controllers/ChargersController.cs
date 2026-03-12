using ChargePointNet.Core;
using ChargePointNet.Models;
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
                .Select(x => new Charger
                {
                    Serial = x.Serial,
                    Status = x.Status,
                    HardwareVersion = x.HardwareVersion,
                    FirmwareVersion = x.FirmwareVersion,
                    Meter = x.Meter != null ? new ChargerMeter
                    {
                        Version = x.Meter.Version,
                        Model = x.Meter.Model,
                        Serial = x.Meter.Serial,
                        MainsFrequency = x.Meter.MainsFrequency
                    } : null
                })
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
            return NotFound(new ErrorResponse
            {
                Message = $"Charger with serial {serial} not found."
            });
        }

        return Ok(new Charger
        {
            Serial = charger.Serial,
            Status = charger.Status,
            HardwareVersion = charger.HardwareVersion,
            FirmwareVersion = charger.FirmwareVersion,
            Meter = charger.Meter != null ? new ChargerMeter
            {
                Version = charger.Meter.Version,
                Model = charger.Meter.Model,
                Serial = charger.Meter.Serial,
                MainsFrequency = charger.Meter.MainsFrequency
            } : null
        });
    }
    
    /// <summary>
    ///     Update led brightness
    /// </summary>
    /// <param name="serial">The serial of a charger.</param>
    /// <param name="request">Test123</param>
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
            return NotFound(new ErrorResponse
            {
                Message = $"Charger with serial {serial} not found."
            });
        }
        
        charger.UpdateLedBrightness(request.Brightness);
        
        return Ok();
    }
}