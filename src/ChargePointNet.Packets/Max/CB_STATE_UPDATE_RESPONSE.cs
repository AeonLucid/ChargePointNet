namespace ChargePointNet.Packets.Max;

public partial class CB_STATE_UPDATE_RESPONSE : IHexPacket
{
    public uint SessionId { get; set; }
    public uint Timestamp { get; set; }
}