namespace ChargePointNet.Packets.Max;

public partial class CONNECTION_STATE_CHANGED_REQUEST : IHexPacket
{
    private byte Unknown_0 { get; set; }
    public ushort HeartbeatInterval { get; set; }
    private byte Unknown_6 { get; set; }
    public byte LedEnable { get; set; }
}