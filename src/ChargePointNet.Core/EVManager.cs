using ChargePointNet.Core.Net;
using ChargePointNet.Core.Protocols;
using ChargePointNet.Core.Protocols.Max;
using Serilog;

namespace ChargePointNet.Core;

public class EVManager : IDisposable
{
    private static readonly ILogger Logger = Log.ForContext<EVManager>();
    
    private readonly Dictionary<string, IModem> _modems;
    private readonly Dictionary<string, IChargeBox> _boxes;
    private readonly List<ITickable> _tickers;

    private Task? _tickTask;
    private bool _stopped;
    private bool _disposed;
    
    public EVManager()
    {
        _modems = [];
        _boxes = [];
        _tickers = [];
    }

    public void Start()
    {
        if (_tickTask != null)
        {
            return;
        }
        
        _tickTask = Task.Run(TickLoop);
    }
    
    public void Stop()
    {
        if (_stopped)
        {
            return;
        }
        
        _stopped = true;
    }

    private async Task TickLoop()
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        
        try
        {
            while (await timer.WaitForNextTickAsync())
            {
                if (_stopped)
                {
                    break;
                }

                Cleanup();

                foreach (var tickable in _tickers)
                {
                    tickable.Tick();
                }
            }
        }
        catch (Exception e)
        {
            Logger.Error(e, "Tick loop cancelled due to exception");
        }
    }

    public bool IsRegistered(string portName)
    {
        return _modems.ContainsKey(portName) || _boxes.ContainsKey(portName);
    }

    public void RegisterDevice(IDevice device)
    {
        object? newEntry;

        switch (device.Protocol)
        {
            case EVProtocol.Max:
                _modems[device.Identifier] = new MaxModem(device);
                _modems[device.Identifier].Start();

                newEntry = _modems[device.Identifier];
                break;
            default:
                throw new NotSupportedException($"Protocol {device.Protocol} is not supported");
        }

        if (newEntry is ITickable tickable)
        {
            _tickers.Add(tickable);
        }
    }

    private void Cleanup()
    {
        foreach (var modem in _modems)
        {
            if (modem.Value.Connected)
            {
                continue;
            }

            Logger.Warning("Lost connection with modem on port {ModemPort}", modem.Key);

            _modems.Remove(modem.Key);

            if (modem.Value is ITickable tickable)
            {
                _tickers.Remove(tickable);
            }

            modem.Value.Dispose();
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        
        _disposed = true;
        
        Stop();
    }
}