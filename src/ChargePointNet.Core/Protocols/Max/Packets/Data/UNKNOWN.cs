using ChargePointNet.Core.Protocols.Max.Packets.Serialization;

namespace ChargePointNet.Core.Protocols.Max.Packets.Data;

internal class UNKNOWN : IMaxPacketData
{
    public byte[] Data { get; set; } = [];
    
    public int Size()
    {
        return Data.Length;
    }

    public void Serialize(ref SpanWriter writer)
    {
        writer.WriteBytes(Data);
    }

    public bool Deserialize(ref SpanReader reader)
    {
        if (reader.TryReadBytes(reader.Remaining, out var bytes))
        {
            Data = bytes;
            return true;
        }
        
        return false;
    }
}