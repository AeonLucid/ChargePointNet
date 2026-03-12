namespace ChargePointNet.Models;

public class ChargerMeter
{
    public required string? Version { get; init; }
    public required string? Model { get; init; }
    public required string? Serial { get; init; }
    public required double? MainsFrequency { get; init; }
}