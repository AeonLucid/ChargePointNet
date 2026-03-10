namespace ChargePointNet.Packets.Max;

public partial class CHARGING_STATE_RESPONSE : IHexPacket
{
    /// <summary>
    ///     0xAA00 = ACK
    ///     0x0055 = NACK
    /// </summary>
    public ushort Ack { get; set; }
}