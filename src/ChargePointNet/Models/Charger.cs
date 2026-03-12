using System.Text.Json.Serialization;
using ChargePointNet.Core.Data;

namespace ChargePointNet.Models;

/// <summary>
///     Test3
/// </summary>
public class Charger
{
    public required string Serial { get; init; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required ChargerBoxStatus Status { get; init; }
    public required string HardwareVersion { get; init; }
    public required string FirmwareVersion { get; init; }
    public required ChargerMeter? Meter { get; init; }
}