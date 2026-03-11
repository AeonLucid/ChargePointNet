namespace ChargePointNet.Packets.Max;

public partial class GET_CB_CONFIGURATION_RESPONSE : IHexPacket
{
    [PacketField(FixedSize = 20, Raw = true)]
    private byte[]? Unknown_00 { get; set; }
    /// <summary>
    ///     In seconds.
    /// </summary>
    public ushort MeterUpdateInterval { get; set; }
    [PacketField(FixedSize = 6, Raw = true)]
    private byte[]? Unknown_24 { get; set; }
    public byte MeterType { get; set; }
    [PacketField(FixedSize = 4, Raw = true)]
    private byte[]? Unknown_32 { get; set; }
    public byte LedBrightness { get; set; }
    [PacketField(FixedSize = 16, Raw = true)]
    private byte[]? Unknown_38 { get; set; }
    public byte AutoStart { get; set; }
    [PacketField(FixedSize = 10, Raw = true)]
    private byte[]? Unknown_56 { get; set; }
    public byte RemoteStart { get; set; }
    // Unknown trailing data.
}