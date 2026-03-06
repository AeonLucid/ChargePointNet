using System.Net;
using System.Net.Sockets;

namespace ChargePointNet.Core.Net.Devices;

public class NetworkDevice : IDevice
{
    private readonly IPEndPoint _endPoint;
    private readonly Socket _socket;

    private bool _reachedEndOfStream;

    public NetworkDevice(IPEndPoint endPoint, Socket socket)
    {
        _endPoint = endPoint;
        _socket = socket;
        
        Identifier = _endPoint.ToString();
    }
    
    public string Identifier { get; }
    public bool Connected => _socket.Connected && !_reachedEndOfStream;

    public async Task<bool> ConnectAsync()
    {
        try
        {
            await _socket.ConnectAsync(_endPoint);
            return true;
        }
        catch (SocketException)
        {
            return false;
        }
    }

    public async Task<int> ReadAsync(Memory<byte> buffer)
    {
        if (_socket.Poll(1000, SelectMode.SelectRead))
        {
            var read = await _socket.ReceiveAsync(buffer);
            if (read == 0)
            {
                _reachedEndOfStream = true;
            }

            return read;
        }

        return 0;
    }

    public async Task WriteAsync(ReadOnlyMemory<byte> buffer)
    {
        await _socket.SendAsync(buffer);
    }

    public void Close()
    {
        _socket.Close();
    }

    public void Dispose()
    {
        _socket.Dispose();
    }

    public override string ToString()
    {
        return $"NetworkDevice: {Identifier}";
    }
}