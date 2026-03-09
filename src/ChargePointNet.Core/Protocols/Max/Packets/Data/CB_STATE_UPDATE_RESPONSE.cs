using ChargePointNet.Core.Protocols.Max.Packets.Serialization;

namespace ChargePointNet.Core.Protocols.Max.Packets.Data;

internal class CB_STATE_UPDATE_RESPONSE : IMaxPacketData
{
    public uint SessionId { get; set; }
    public uint Timestamp { get; set; }
    
    public int Size()
    {
        return 16;
    }

    public void Serialize(ref SpanWriter writer)
    {
        writer.WriteU32Hex(SessionId);
        writer.WriteU32Hex(Timestamp);
    }

    public bool Deserialize(ref SpanReader reader)
    {
        throw new NotImplementedException();
    }
}