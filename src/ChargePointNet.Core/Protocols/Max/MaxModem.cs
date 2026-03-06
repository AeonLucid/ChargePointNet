using System.IO.Ports;
using Serilog;

namespace ChargePointNet.Core.Protocols.Max;

/// <summary>
///     Implements a max modem. See https://www.geekabit.nl/projects/managed-ev-charger-to-stand-alone/protocol/
/// </summary>
public class MaxModem : IModem
{
    private static readonly ILogger Logger = Log.ForContext<MaxModem>();
    
    private readonly SerialPort _port;
    private readonly MaxModemBus _bus;

    private bool _disposed;

    public MaxModem(SerialPort port)
    {
        _port = port;
        _bus = new MaxModemBus(port);
    }
    
    public bool Connected => _bus.Connected;

    public void Start()
    {
        _bus.Start();
        _bus.Send(new MaxPacket(Convert.FromHexString("024243383031453633374403FF")));
    }

    public void Stop()
    {
        _bus.Stop();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        
        _disposed = true;
        _bus.Dispose();
    }
}