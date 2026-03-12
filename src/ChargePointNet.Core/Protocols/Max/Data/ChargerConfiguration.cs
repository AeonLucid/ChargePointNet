namespace ChargePointNet.Core.Protocols.Max.Data;

public class ChargerConfiguration
{
    public required ushort MeterUpdateInterval { get; init; }
    public required MaxMeterType MeterType { get; init; }
    public required byte LedBrightness { get; set; }
    public required byte AutoStart { get; init; }
    public required byte RemoteStart { get; init; }
}