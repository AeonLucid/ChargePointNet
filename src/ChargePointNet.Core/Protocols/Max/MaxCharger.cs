using ChargePointNet.Core.Protocols.Max.Data;
using ChargePointNet.Core.Protocols.Max.Packets;
using ChargePointNet.Packets;
using ChargePointNet.Packets.Max;
using Serilog;

namespace ChargePointNet.Core.Protocols.Max;

public class MaxCharger : IChargeBox, ITickable
{
    private static readonly ILogger Logger = Log.ForContext<MaxCharger>();
    
    private readonly MaxModem _modem;

    private bool _isConfigurationAcknowledged;
    
    internal MaxCharger(MaxModem modem)
    {
        _modem = modem;
    }

    public required byte Address { get; init; }
    public required string Serial { get; init; }
    public required string HardwareVersion { get; init; }
    public required string FirmwareVersion { get; init;}
    public string HardwareGeneration => HardwareVersion[^2..];

    public ChargerBoxState? BoxState { get; private set; }
    public ChargingState? ChargingState { get; private set; }
    public MeterInfo? MeterInfo { get; private set; }
    public ChargerConfiguration? ChargerConfiguration { get; private set; }
    
    public void Initialize()
    {
        Send(MaxCommand.CONNECTION_STATE_CHANGED, new CONNECTION_STATE_CHANGED_REQUEST
        {
            HeartbeatInterval = 900 * 256,
            LedEnable = 1
        });
    }

    void ITickable.Tick()
    {
        if (!ConfigureCharger())
        {
            return;
        }
        
        // Configured properly.
    }

    internal void Send(MaxCommand command, IHexPacket? data = null)
    {
        _modem.SendTo(Address, command, data);
    }

    /// <summary>
    ///     Ensures that the charger is configured correctly.
    /// </summary>
    /// <returns>Returns false if the charger is pending configuration.</returns>
    private bool ConfigureCharger()
    {
        // Check if initialized.
        if (ChargingState == null)
        {
            return false;
        }
        
        // Fetch meter info.
        if (MeterInfo == null)
        {
            Send(MaxCommand.GET_METER_INFO);
            return false;
        }
        
        // Fetch charger configuration.
        if (ChargerConfiguration == null)
        {
            Send(MaxCommand.GET_CB_CONFIGURATION);
            return false;
        }
        
        // Check if the configuration has been acknowledged.
        if (!_isConfigurationAcknowledged)
        {
            Send(MaxCommand.SET_CB_CONFIGURATION, new SET_CB_CONFIGURATION_REQUEST
            {
                Mask = 0xFFFFFFFF,
                LedBrightness = 100,
                MeterType = 0,
                AutoStart = 0,
                Unknown_54 = 900,
                MeterUpdateInterval = 900,
                RemoteStart = 0,
                Unknown_10 = "030000"u8.ToArray(),
                Unknown_18 = "01000100000000000000"u8.ToArray(),
                Unknown_40 = "000000003C0000"u8.ToArray(),
                Unknown_58 = "0000"u8.ToArray(),
                Unknown_64 = "01000000"u8.ToArray(),
                Unknown_76 = "03E8010000"u8.ToArray()
            });
            return false;
        }

        return true;
    }
    
    internal void OnPacketReceived(MaxPacket packet)
    {
        switch (packet.Data)
        {
            case CHARGING_STATE_REQUEST request:
            {
                Send(MaxCommand.CHARGING_STATE, new CHARGING_STATE_RESPONSE
                {
                    Ack = MaxAcknowledgment.ACK
                });
                
                ChargingState = (ChargingState)request.State;
                break;
            }
            case CB_STATE_UPDATE_REQUEST request:
            {
                Send(MaxCommand.CB_STATE_UPDATE, new CB_STATE_UPDATE_RESPONSE
                {
                    SessionId = 0,
                    Timestamp = MaxUtils.Timestamp()
                });
                
                BoxState = (ChargerBoxState)request.State;
                break;
            }
            case GET_METER_INFO_RESPONSE response:
            {
                MeterInfo = new MeterInfo
                {
                    Version = response.VersionNumber![..response.VersionNumberLength],
                    Model = response.ModelName![..response.ModelNameLength],
                    Serial = response.SerialNumber!,
                    MainsFrequency = response.MainsFrequency / 100d,
                };
                break;
            }
            case GET_CB_CONFIGURATION_RESPONSE response:
            {
                ChargerConfiguration = new ChargerConfiguration
                {
                    MeterUpdateInterval = response.MeterUpdateInterval,
                    MeterType = (MeterType)response.MeterType,
                    LedBrightness = response.LedBrightness,
                    AutoStart = response.AutoStart,
                    RemoteStart = response.RemoteStart,
                };
                break;
            }
            case SET_CB_CONFIGURATION_RESPONSE response:
            {
                _isConfigurationAcknowledged = response.Ack == MaxAcknowledgment.ACK;
                break;
            }
            default:
            {
                Logger.Warning("Unhandled charger packet {Command} {DataType}", packet.Command, packet.Data?.GetType().Name);
                break;
            }
        }
    }

    public void Dispose()
    {
        
    }
}