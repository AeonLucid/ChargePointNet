namespace ChargePointNet.Core.Protocols.Max.Packets;

internal class MaxPacket
{
    public required byte Destination { get; set; }
    public required byte Source { get; set; }
    public required MaxCommand Command { get; set; }
    public IMaxPacketData? Data { get; set; }
}