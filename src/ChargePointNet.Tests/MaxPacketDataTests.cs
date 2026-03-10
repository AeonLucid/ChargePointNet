using ChargePointNet.Packets;
using ChargePointNet.Packets.Max;

namespace ChargePointNet.Tests;

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
    public void StateUpdateRequest()
    {
        var request = new CB_STATE_UPDATE_REQUEST
        {
            
        };
        
        var requestData = SerializeData(request);
        
        Assert.That(requestData.Length, Is.EqualTo(132));
    }
    
    [Test]
    public void StateUpdateResponse()
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

    private static byte[] SerializeData(IHexPacket data)
    {
        var packetSize = data.Size();
        var packetBuffer = new byte[packetSize];
        var writer = new HexSpanWriter(packetBuffer);
        
        data.Serialize(ref writer);

        return packetBuffer;
    }
}