using System.Buffers;
using ChargePointNet.Core.Protocols.Max;
using ChargePointNet.Core.Protocols.Max.Packets;

namespace ChargePointNet.Core.Tests;

public class MaxPacketFrameTests
{
    [Test]
    public void TestChecksum()
    {
        var payload = "800011666666601250003"u8;
        var checksum = "2F"u8;

        Span<byte> checksumBuffer = stackalloc byte[2];
        MaxPacketFrame.CalculateChecksum(payload, checksumBuffer);

        Assert.That(checksumBuffer.SequenceEqual(checksum), Is.True);
    }

    [Test]
    public void TestParity()
    {
        var payload = "800011666666601250003"u8;
        var checksum = "3B"u8;

        Span<byte> checksumBuffer = stackalloc byte[2];
        MaxPacketFrame.CalculateParity(payload, checksumBuffer);

        Assert.That(checksumBuffer.SequenceEqual(checksum), Is.True);
    }

    [Test]
    public void TestReadPayload()
    {
        var packet = Convert.FromHexString("023830303031313636363636363630313235303030333246334203ff");
        var packetSequence = new ReadOnlySequence<byte>(packet);

        var result = MaxPacketFrame.TryReadPacketFrame(packetSequence, out var frameResult, out var error);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(error, Is.Null);
            Assert.That(frameResult.Length, Is.EqualTo(21));
        });
    }
}