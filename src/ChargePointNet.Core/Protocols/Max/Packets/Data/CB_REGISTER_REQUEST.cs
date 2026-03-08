using System.Text;
using ChargePointNet.Core.Protocols.Max.Packets.Serialization;

namespace ChargePointNet.Core.Protocols.Max.Packets.Data;

internal class CB_REGISTER_REQUEST : IMaxPacketData
{
    public string? Serial { get; set; }
    public string? FirmwareVersion { get; set; }
    public string? HardwareVersion { get; set; }
    
    public int Size()
    {
        return 15;
    }

    public void Serialize(ref SpanWriter writer)
    {
        throw new NotImplementedException();
    }

    public bool Deserialize(ref SpanReader reader)
    {
        if (!reader.TryReadString(Encoding.ASCII, 7, out var value))
        {
            return false;
        }
        
        Serial = value;
        
        if (!reader.TryReadString(Encoding.ASCII, 4, out value))
        {
            return false;
        }

        FirmwareVersion = value;
        
        if (!reader.TryReadString(Encoding.ASCII, 4, out value))
        {
            return false;
        }

        HardwareVersion = value;
        return true;
    }
}