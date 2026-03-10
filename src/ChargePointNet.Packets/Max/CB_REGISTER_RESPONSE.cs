namespace ChargePointNet.Packets.Max;

public partial class CB_REGISTER_RESPONSE : IHexPacket
{
    [PacketField(FixedSize = 7)]
    public string? Serial { get; set; }
    public byte Address { get; set; }
    [PacketField(FixedSize = 2)]
    public string? HardwareGeneration { get; set; }
}