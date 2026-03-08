using ChargePointNet.Core.Protocols.Max.Packets.Serialization;

namespace ChargePointNet.Core.Protocols.Max.Packets;

internal interface IMaxPacketData
{
    int Size();
    void Serialize(ref SpanWriter writer);
    bool Deserialize(ref SpanReader reader);
}