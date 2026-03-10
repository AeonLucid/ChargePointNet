namespace ChargePointNet.Packets.Max;

public partial class CB_REGISTER_REQUEST : IHexPacket
{
    [PacketField(FixedSize = 7)]
    public string? Serial { get; set; }
    [PacketField(FixedSize = 4)]
    public string? FirmwareVersion { get; set; }
    [PacketField(FixedSize = 4)]
    public string? HardwareVersion { get; set; }
}