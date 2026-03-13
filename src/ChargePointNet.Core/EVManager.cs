using ChargePointNet.Core.Interfaces;
using ChargePointNet.Core.Net;
using ChargePointNet.Core.Protocols;
using ChargePointNet.Core.Protocols.Max;
using Serilog;

namespace ChargePointNet.Core;

public class EVManager : IDisposable
{
    private static readonly ILogger Logger = Log.ForContext<EVManager>();
    
    private readonly IAuthRepository _authRepository;
    private readonly ISessionRepository _sessionRepository;

    private readonly Dictionary<string, IModem> _modems;
    private readonly Dictionary<string, IChargeBox> _boxes;
    private readonly List<ITickable> _tickers;

    private Task? _tickTask;
    private bool _stopped;
    private bool _disposed;
    
    public EVManager(IAuthRepository authRepository, ISessionRepository sessionRepository)
    {
        _authRepository = authRepository;
        _sessionRepository = sessionRepository;
        _modems = [];
        _boxes = [];
        _tickers = [];
    }
    
    public IEnumerable<IChargeBox> ChargeBoxes => _modems
        .Where(x => x.Value.Connected)
        .SelectMany(x => x.Value.Chargers)
        .Concat(_boxes.Values);
    
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
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(500));
        
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
                _modems[device.Identifier] = new MaxModem(device, _authRepository, _sessionRepository);
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

    public IChargeBox? FindBySerial(string serial)
    {
        return ChargeBoxes.FirstOrDefault(x => x.Serial == serial);
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