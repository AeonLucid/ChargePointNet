using System.ComponentModel.DataAnnotations;

namespace ChargePointNet.Models.Requests;

public class LedBrightnessUpdateRequest
{
    /// <summary>
    ///     Brightness of the LEDs in percent (0-100).
    /// </summary>
    [Range(0, 100)]
    public required byte Brightness { get; set; }
}