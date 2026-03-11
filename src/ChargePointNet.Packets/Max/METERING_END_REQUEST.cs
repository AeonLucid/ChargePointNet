namespace ChargePointNet.Packets.Max;

public partial class METERING_END_REQUEST : IHexPacket
{
    public byte CardNumberLength { get; set; }
    [PacketField(FixedSize = 22)]
    public string? CardNumberValue { get; set; }
    /// <summary>
    ///     <inheritdoc cref="METERING_START_REQUEST.MeterValue"/>
    /// </summary>
    public uint MeterValue { get; set; }
    public uint SessionId { get; set; }
    public byte State { get; set; }
    public uint Timestamp { get; set; }
}