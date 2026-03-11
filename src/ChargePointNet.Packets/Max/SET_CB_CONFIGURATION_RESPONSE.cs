namespace ChargePointNet.Packets.Max;

public partial class SET_CB_CONFIGURATION_RESPONSE : IHexPacket
{
    public ushort Ack { get; set; }
}