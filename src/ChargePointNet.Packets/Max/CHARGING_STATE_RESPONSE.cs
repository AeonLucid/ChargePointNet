namespace ChargePointNet.Packets.Max;

public partial class CHARGING_STATE_RESPONSE : IHexPacket
{
    public ushort Ack { get; set; }
}