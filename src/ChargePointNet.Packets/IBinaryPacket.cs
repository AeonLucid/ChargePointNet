namespace ChargePointNet.Packets;

/// <summary>
///     Normal binary data packet.
/// </summary>
public interface IBinaryPacket
{
    int Size();
    void Serialize(ref HexSpanWriter writer);
    bool Deserialize(ref HexSpanReader reader);
}