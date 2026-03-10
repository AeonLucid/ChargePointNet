using System.Collections.Concurrent;
using ChargePointNet.Core.Net;
using ChargePointNet.Core.Protocols.Max.Packets;
using ChargePointNet.Packets;
using ChargePointNet.Packets.Max;
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
            Destination = MaxAddress.BROADCAST,
            Source = MaxAddress.MODEM,
            Command = MaxCommand.RESTART_REGISTRATION
        });
    }

    public void Stop()
    {
        _bus.Stop();
    }
    
    internal void SendTo(byte address, MaxCommand command, IHexPacket? data = null)
    {
        _bus.Send(new MaxPacket
        {
            Destination = address,
            Source = MaxAddress.MODEM,
            Command = command,
            Data = data
        });
    }

    internal void SendBroadcast(MaxCommand command, IHexPacket? data = null)
    {
        _bus.Send(new MaxPacket
        {
            Destination = MaxAddress.BROADCAST,
            Source = MaxAddress.MODEM,
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
        
        for (var i = MaxAddress.CB_MIN; i <= MaxAddress.CB_MAX; i++)
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
            return Task.CompletedTask;
        }

        if (packet.Source >= MaxAddress.CB_MIN && packet.Source <= MaxAddress.CB_MAX)
        {
            if (_chargers.TryGetValue(packet.Source, out var charger))
            {
                charger.OnPacketReceived(packet);
            }
            else
            {
                Logger.Warning("Received packet for unregistered charger at address {Address}", packet.Source);
            }
            
            return Task.CompletedTask;
        }

        Logger.Warning("Received packet for unknown address {Address}", packet.Source);
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