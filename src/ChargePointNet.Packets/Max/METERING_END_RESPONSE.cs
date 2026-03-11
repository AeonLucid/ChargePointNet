namespace ChargePointNet.Packets.Max;

public partial class METERING_END_RESPONSE : IHexPacket
{
    public byte State { get; set; }
}