namespace ChargePointNet.Packets.Max;

public partial class METERING_START_REQUEST : IHexPacket
{
    public byte CardNumberLength { get; set; }
    [PacketField(FixedSize = 22)]
    public string? CardNumberValue { get; set; }
    /// <summary>
    ///     Divide by 1000 to get the actual value in kWh.
    ///     For example, a value of 12345 represents 12.345 kWh.
    /// </summary>
    public uint MeterValue { get; set; }
}