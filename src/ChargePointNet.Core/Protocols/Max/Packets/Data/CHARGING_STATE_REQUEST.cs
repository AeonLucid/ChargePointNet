using ChargePointNet.Core.Protocols.Max.Data;
using ChargePointNet.Core.Protocols.Max.Packets.Serialization;

namespace ChargePointNet.Core.Protocols.Max.Packets.Data;

internal class CHARGING_STATE_REQUEST : IMaxPacketData
{
    public ChargingState State { get; set; }
    public byte Unknown { get; set; }
    
    public int Size()
    {
        return 4;
    }

    public void Serialize(ref SpanWriter writer)
    {
        throw new NotImplementedException();
    }

    public bool Deserialize(ref SpanReader reader)
    {
        if (!reader.TryReadU8(out var value))
        {
            return false;
        }
        
        State = (ChargingState)value;
        
        if (!reader.TryReadU8(out value))
        {
            return false;
        }

        Unknown = value;
        return true;
    }
}