using ChargePointNet.Packets;

namespace ChargePointNet.Tests;

public class HexIOTests
{
    [Test]
    public void TestConsistency()
    {
        var buffer = new byte[16];
        
        var reader = new HexSpanReader(buffer);
        var writer = new HexSpanWriter(buffer);
        
        writer.WriteU8(1);
        writer.WriteU16(0x0102);
        writer.WriteU32(0x01020304);
        
        Assert.That(reader.TryReadU8(out var value1));
        Assert.That(value1, Is.EqualTo(1));
        
        Assert.That(reader.TryReadU16(out var value2));
        Assert.That(value2, Is.EqualTo(0x0102));
        
        Assert.That(reader.TryReadU32(out var value3));
        Assert.That(value3, Is.EqualTo(0x01020304));
    }
}