namespace ChargePointNet.Packets.Max;

public partial class METERING_START_RESPONSE : IHexPacket
{
    public byte State { get; set; }
    public uint SessionId { get; set; }
    public uint Timestamp { get; set; }
}