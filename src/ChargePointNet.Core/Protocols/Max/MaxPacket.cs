namespace ChargePointNet.Core.Protocols.Max;

public class MaxPacket
{
    private readonly byte[] _data;

    public MaxPacket(byte[] data)
    {
        _data = data;
    }
    
    public byte[] GetData() => _data;
}