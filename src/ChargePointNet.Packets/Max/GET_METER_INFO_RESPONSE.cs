namespace ChargePointNet.Packets.Max;

public partial class GET_METER_INFO_RESPONSE : IHexPacket
{
    /// <summary>
    ///     0xAA00 = ACK
    ///     0x0055 = NACK
    /// </summary>
    public ushort MeterPresent { get; set; }
    public byte VersionNumberLength { get; set; }
    [PacketField(FixedSize = 16)]
    public string? VersionNumber { get; set; }
    public byte ModelNameLength { get; set; }
    [PacketField(FixedSize = 16)]
    public string? ModelName { get; set; }
    [PacketField(FixedSize = 16)]
    public string? SerialNumber { get; set; }
    public ushort MainsFrequency { get; set; }
    [PacketField(FixedSize = 4, Raw = true)]
    public byte[]? Unknown { get; set; }
}