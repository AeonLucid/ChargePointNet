using ChargePointNet.Core.Protocols.Max.Packets.Serialization;

namespace ChargePointNet.Core.Protocols.Max.Packets.Data;

internal class CONNECTION_STATE_CHANGED_REQUEST : IMaxPacketData
{
    public int? HeartbeatInterval { get; set; }
    public byte? LedEnable { get; set; }
    
    public int Size()
    {
        return 10;
    }

    public void Serialize(ref SpanWriter writer)
    {
        writer.WriteU32Hex(HeartbeatInterval ?? 0);
        writer.WriteU8Hex(LedEnable ?? 0);
    }

    public bool Deserialize(ref SpanReader reader)
    {
        throw new NotImplementedException();
    }
}