using ChargePointNet.Core.Protocols.Max.Packets.Serialization;

namespace ChargePointNet.Core.Protocols.Max.Packets.Data;

internal class CHARGING_STATE_RESPONSE : IMaxPacketData
{
    public ushort Ack { get; set; }
    
    public int Size()
    {
        return 4;
    }

    public void Serialize(ref SpanWriter writer)
    {
        writer.WriteU16Hex(Ack);
    }

    public bool Deserialize(ref SpanReader reader)
    {
        throw new NotImplementedException();
    }
}