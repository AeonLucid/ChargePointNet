using ChargePointNet.Core.Protocols;

namespace ChargePointNet.Core.Net;

public interface IDevice : IDisposable
{
    /// <summary>
    ///     Unique identifier for the device.
    /// </summary>
    string Identifier { get; }
    
    /// <summary>
    ///     The protocol that the device uses for communication.
    /// </summary>
    EVProtocol Protocol { get; }
    
    /// <summary>
    ///     Whether the device is currently available for communication.
    /// </summary>
    bool Connected { get; }

    Task<bool> ConnectAsync();
    
    Task<int> ReadAsync(Memory<byte> buffer);
    
    Task WriteAsync(ReadOnlyMemory<byte> buffer);
    
    void Close();
}