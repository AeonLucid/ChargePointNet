using System.Collections.Concurrent;
using ChargePointNet.Core.Net;
using ChargePointNet.Core.Protocols.Max.Packets;
using ChargePointNet.Core.Protocols.Max.Packets.Data;
using Serilog;

namespace ChargePointNet.Core.Protocols.Max;

/// <summary>
///     Implements a max modem. See https://www.geekabit.nl/projects/managed-ev-charger-to-stand-alone/protocol/
/// </summary>
public class MaxModem : IModem
{
    private static readonly ILogger Logger = Log.ForContext<MaxModem>();

    private const byte ADDRESS_NEW = 0x00;
    private const byte ADDRESS_CB_MIN = 0x01;
    private const byte ADDRESS_CB_MAX = 0x01 + 20 - 1;
    private const byte ADDRESS_MODEM = 0x80;
    private const byte ADDRESS_BROADCAST = 0xBC;
    private const byte ADDRESS_CHARGE_STATION = 0xFD;
    
    private readonly IDevice _device;
    private readonly MaxModemBus _bus;
    private readonly ConcurrentDictionary<byte, MaxCharger> _chargers;

    private bool _disposed;

    public MaxModem(IDevice device)
    {
        _device = device;
        _bus = new MaxModemBus(device);
        _bus.OnPacketReceived += OnPacketReceived;
        _chargers = [];
    }

    public bool Connected => _bus.Connected;

    public void Start()
    {
        _bus.Start();
        _bus.Send(new MaxPacket
        {
            Destination = ADDRESS_BROADCAST,
            Source = ADDRESS_MODEM,
            Command = MaxCommand.RESTART_REGISTRATION
        });
    }

    public void Stop()
    {
        _bus.Stop();
    }
    
    internal void SendTo(byte address, MaxCommand command, IMaxPacketData data)
    {
        _bus.Send(new MaxPacket
        {
            Destination = address,
            Source = ADDRESS_MODEM,
            Command = command,
            Data = data
        });
    }

    internal void SendBroadcast(MaxCommand command, IMaxPacketData data)
    {
        _bus.Send(new MaxPacket
        {
            Destination = ADDRESS_BROADCAST,
            Source = ADDRESS_MODEM,
            Command = command,
            Data = data
        });
    }

    private MaxCharger? AddCharger(string? serial, string? hardwareVersion, string? firmwareVersion)
    {
        if (serial == null || hardwareVersion == null || firmwareVersion == null)
        {
            Logger.Debug("Failed to add charger, missing fields");
            return null;
        }
        
        byte? address = null;
        
        for (var i = ADDRESS_CB_MIN; i <= ADDRESS_CB_MAX; i++)
        {
            if (_chargers.ContainsKey(i))
            {
                continue;
            }

            address = i;
            break;
        }
        
        if (address == null)
        {
            Logger.Warning("No more addresses available for new charger");
            return null;
        }

        var charger = new MaxCharger(this)
        {
            Address = address.Value,
            Serial = serial,
            HardwareVersion = hardwareVersion,
            FirmwareVersion = firmwareVersion
        };
        
        if (!_chargers.TryAdd(address.Value, charger))
        {
            Logger.Warning("Failed to add charger, address already in use");
            return null;
        }
        
        // TODO: Logging elsewhere when we register chargers in the EVManager.
        Logger.Information("Registered new MaxCharger with address {Address}", address.Value);
        
        return charger;
    }

    private Task OnPacketReceived(MaxPacket packet)
    {
        if (packet.Data is CB_REGISTER_REQUEST regRequest)
        {
            var charger = AddCharger(regRequest.Serial, regRequest.HardwareVersion, regRequest.FirmwareVersion);
            if (charger == null)
            {
                Logger.Warning("Failed to add charger for CB_REGISTER_REQUEST from {Source}", packet.Source);
                return Task.CompletedTask;
            }

            SendBroadcast(MaxCommand.CB_REGISTER, new CB_REGISTER_RESPONSE
            {
                Serial = charger.Serial,
                Address = charger.Address,
                HardwareGeneration = charger.HardwareGeneration
            });

            charger.Initialize();
        }

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        
        _disposed = true;
        _bus.OnPacketReceived -= OnPacketReceived;
        _bus.Dispose();
    }
}