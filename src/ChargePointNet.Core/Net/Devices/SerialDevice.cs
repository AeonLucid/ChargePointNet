using System.IO.Ports;
using ChargePointNet.Core.Protocols;

namespace ChargePointNet.Core.Net.Devices;

public class SerialDevice : IDevice
{
    private const int ReadTimeout = 1000;
    private const int WriteTimeout = 1000;
    private const int BusIdleDelay = 100;
    
    private readonly SerialPort _port;

    public SerialDevice(SerialPort port, EVProtocol protocol)
    {
        _port = port;
        Protocol = protocol;
    }
    
    public string Identifier => _port.PortName;
    public bool Connected => _port.IsOpen;
    public EVProtocol Protocol { get; }

    public Task<bool> ConnectAsync()
    {
        _port.Open();

        if (!_port.IsOpen)
        {
            return Task.FromResult(false);
        }
        
        _port.BaseStream.ReadTimeout = ReadTimeout;
        _port.BaseStream.WriteTimeout = WriteTimeout;
        
        return Task.FromResult(true);
    }

    public async Task<int> ReadAsync(Memory<byte> buffer)
    {
        if (_port.BytesToRead == 0)
        {
            return 0;
        }
        
        var result = await _port.BaseStream.ReadAsync(buffer);
        await Task.Delay(BusIdleDelay);
        return result;
    }
    
    public async Task WriteAsync(ReadOnlyMemory<byte> buffer)
    {
        await _port.BaseStream.WriteAsync(buffer);
        await Task.Delay(BusIdleDelay);
    }

    public void Close()
    {
        _port.Close();
    }

    public void Dispose()
    {
        _port.Dispose();
    }

    public override string ToString()
    {
        return $"(SerialDevice: {Identifier})";
    }
}