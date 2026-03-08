using System.Text;
using ChargePointNet.Core.Protocols.Max.Packets.Serialization;

namespace ChargePointNet.Core.Protocols.Max.Packets.Data;

internal class CB_REGISTER_RESPONSE : IMaxPacketData
{
    public string? Serial { get; set; }
    public byte? Address { get; set; }
    public string? HardwareGeneration { get; set; }
    
    public int Size()
    {
        return 11;
    }

    public void Serialize(ref SpanWriter writer)
    {
        if (Serial == null || Serial.Length != 7)
        {
            throw new InvalidOperationException("Serial must be exactly 7 characters long.");
        }

        if (Address == null)
        {
            throw new InvalidOperationException("Address must be set.");
        }

        if (HardwareGeneration == null || HardwareGeneration.Length != 2)
        {
            throw new InvalidOperationException("HardwareVersion must be exactly 2 characters long.");
        }
        
        writer.WriteString(Encoding.ASCII, Serial);
        writer.WriteU8Hex(Address.Value);
        writer.WriteString(Encoding.ASCII, HardwareGeneration);
    }

    public bool Deserialize(ref SpanReader reader)
    {
        throw new NotImplementedException();
    }
}