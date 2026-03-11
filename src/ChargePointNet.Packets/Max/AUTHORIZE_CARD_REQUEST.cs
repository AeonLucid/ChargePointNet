namespace ChargePointNet.Packets.Max;

public partial class AUTHORIZE_CARD_REQUEST : IHexPacket
{
    public byte State { get; set; }
    public byte CardNumberLength { get; set; }
    [PacketField(FixedSize = 22)]
    public string? CardNumberValue { get; set; }
}