namespace ChargePointNet.Packets.Max;

public partial class CONNECTION_STATE_CHANGED_REQUEST : IHexPacket
{
    public uint HeartbeatInterval { get; set; }
    public byte LedEnable { get; set; }
}