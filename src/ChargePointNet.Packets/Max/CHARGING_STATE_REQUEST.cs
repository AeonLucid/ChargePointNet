namespace ChargePointNet.Packets.Max;

public partial class CHARGING_STATE_REQUEST : IHexPacket
{
    public byte State { get; set; }
    public byte Unknown { get; set; }
}