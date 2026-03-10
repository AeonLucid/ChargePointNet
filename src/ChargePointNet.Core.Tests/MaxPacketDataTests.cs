using ChargePointNet.Core.Protocols.Max.Packets;
using ChargePointNet.Core.Protocols.Max.Packets.Data;
using ChargePointNet.Core.Protocols.Max.Packets.Serialization;

namespace ChargePointNet.Core.Tests;

public class MaxPacketDataTests
{
    [Test]
    public void RegisterResponse()
    {
        var response = new CB_REGISTER_RESPONSE
        {
            Serial = "1234567",
            Address = 0x01,
            HardwareGeneration = "03"
        };
        
        var responseData = SerializeData(response);
        
        Assert.Multiple(() =>
        {
            Assert.That(responseData.Length, Is.EqualTo(11));
            Assert.That(Convert.ToHexStringLower(responseData), Is.EqualTo("3132333435363730313033"));
        });
    }
    
    [Test]
    public void StateUpdateResponseResponse()
    {
        var response = new CB_STATE_UPDATE_RESPONSE
        {
            SessionId = 123456,
            Timestamp = 789123
        };
        
        var responseData = SerializeData(response);
        
        Assert.Multiple(() =>
        {
            Assert.That(responseData.Length, Is.EqualTo(16));
            Assert.That(Convert.ToHexStringLower(responseData), Is.EqualTo("30303031453234303030304330413833"));
        });
    }

    private byte[] SerializeData(IMaxPacketData data)
    {
        var packetSize = data.Size();
        var packetBuffer = new byte[packetSize];
        var writer = new SpanWriter(packetBuffer);
        
        data.Serialize(ref writer);

        return packetBuffer;
    }
}