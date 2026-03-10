namespace ChargePointNet.Packets.Max;

public class UNKNOWN : IHexPacket
{
    public byte[] Data { get; set; } = [];
    
    public int Size()
    {
        return Data.Length;
    }

    public void Serialize(ref HexSpanWriter writer)
    {
        writer.WriteBytes(Data);
    }

    public bool Deserialize(ref HexSpanReader reader)
    {
        if (reader.TryReadBytes(reader.Remaining, out var bytes))
        {
            Data = bytes;
            return true;
        }
        
        return false;
    }
}