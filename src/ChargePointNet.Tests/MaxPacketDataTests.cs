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
            Assert.That(responseData, Has.Length.EqualTo(11));
            Assert.That(Convert.ToHexString(responseData), Is.EqualTo("3132333435363730313033"));
        });
    }
    
    [Test]
    public void StateUpdateRequest()
    {
        var request = new CB_STATE_UPDATE_REQUEST
        {
            
        };
        
        var requestData = SerializeData(request);
        
        Assert.That(requestData, Has.Length.EqualTo(132));
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
            Assert.That(responseData, Has.Length.EqualTo(16));
            Assert.That(Convert.ToHexString(responseData), Is.EqualTo("30303031453234303030304330413833"));
        });
    }
    
    [Test]
    public void SetCbConfiguration()
    {
        var request = new SET_CB_CONFIGURATION_REQUEST
        {
            Mask = 0xFFFFFFFF,
            LedBrightness = 100,
            MeterType = 0,
            AutoStart = 0,
            Unknown_54 = 900,
            MeterUpdateInterval = 900,
            RemoteStart = 0,
            Unknown_72 = 1000,
            Unknown_10 = "030000"u8.ToArray(),
            Unknown_18 = "01000100000000000000"u8.ToArray(),
            Unknown_40 = "000000003C0000"u8.ToArray(),
            Unknown_58 = "0000"u8.ToArray(),
            Unknown_64 = "01000000"u8.ToArray(),
            Unknown_76 = "010000"u8.ToArray()
        };
        
        var requestData = SerializeData(request);
        
        Assert.Multiple(() =>
        {
            Assert.That(requestData, Has.Length.EqualTo(86));
            Assert.That(Convert.ToHexString(requestData), Is.EqualTo("4646464646464646363430333030303030303031303030313030303030303030303030303030303030303030303030303343303030303033383430303030303338343031303030303030303030334538303130303030"));
        });
    }
    
    [Test]
    public void SetCurrentLimitRequest()
    {
        var request = new SET_CURRENT_LIMIT_REQUEST
        {
            Unknown = 1,
            MinimumCurrent = 60,
            CurrentLimitPhase1 = 60,
            CurrentLimitPhase2 = 60,
            CurrentLimitPhase3 = 60
        };
        
        var requestData = SerializeData(request);
        
        Assert.Multiple(() =>
        {
            Assert.That(requestData, Has.Length.EqualTo(18));
            Assert.That(Convert.ToHexString(requestData), Is.EqualTo("303130303343303033433030334330303343"));
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