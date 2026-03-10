namespace ChargePointNet.Packets;

/// <summary>
///     Hex encoded binary packet, used in Max protocol.
/// </summary>
public interface IHexPacket
{
    int Size();
    void Serialize(ref HexSpanWriter writer);
    bool Deserialize(ref HexSpanReader reader);
}