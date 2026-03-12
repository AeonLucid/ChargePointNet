using System.ComponentModel.DataAnnotations;

namespace ChargePointNet.Models;

public class LedBrightnessUpdateRequest
{
    [Range(0, 100)]
    public required byte Brightness { get; set; }
}