namespace ChargePointNet.Packets.Max;

public partial class SET_CB_CONFIGURATION_REQUEST : IHexPacket
{
    public uint Mask { get; set; }
    public byte LedBrightness { get; set; }
    [PacketField(FixedSize = 6, Raw = true)]
    public byte[]? Unknown_10 { get; set; }
    public byte MeterType { get; set; }
    [PacketField(FixedSize = 20, Raw = true)]
    public byte[]? Unknown_18 { get; set; }
    public byte AutoStart { get; set; }
    [PacketField(FixedSize = 14, Raw = true)]
    public byte[]? Unknown_40 { get; set; }
    public ushort Unknown_54 { get; set; }
    [PacketField(FixedSize = 4, Raw = true)]
    public byte[]? Unknown_58 { get; set; }
    public ushort MeterUpdateInterval { get; set; }
    [PacketField(FixedSize = 8, Raw = true)]
    public byte[]? Unknown_64 { get; set; }
    public byte RemoteStart { get; set; }
    /// <summary>
    ///     Unknown trailing data, size 94 is the highest for firmware 140.
    ///     This way we support all with a single packet.
    /// </summary>
    [PacketField(FixedSize = 10, Raw = true)]
    public byte[]? Unknown_76 { get; set; }
}