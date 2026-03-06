using ChargePointNet.Core.Net;
using Serilog;

namespace ChargePointNet.Core.Protocols.Max;

/// <summary>
///     Implements a max modem. See https://www.geekabit.nl/projects/managed-ev-charger-to-stand-alone/protocol/
/// </summary>
public class MaxModem : IModem
{
    private static readonly ILogger Logger = Log.ForContext<MaxModem>();
    
    private readonly IDevice _device;
    private readonly MaxModemBus _bus;

    private bool _disposed;

    public MaxModem(IDevice device)
    {
        _device = device;
        _bus = new MaxModemBus(device);
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