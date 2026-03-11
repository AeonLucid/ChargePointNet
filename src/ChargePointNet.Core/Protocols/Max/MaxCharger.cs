using ChargePointNet.Core.Protocols.Max.Data;
using ChargePointNet.Core.Protocols.Max.Packets;
using ChargePointNet.Packets;
using ChargePointNet.Packets.Max;
using Serilog;

namespace ChargePointNet.Core.Protocols.Max;

public class MaxCharger : IChargeBox, ITickable
{
    private static readonly ILogger Logger = Log.ForContext<MaxCharger>();
    
    private static readonly CurrentLimit LimitLow = new(60, 60, 60, 60);
    private static readonly CurrentLimit LimitHigh = new(60, 160, 160, 160);
    
    private readonly MaxModem _modem;

    private bool _isConfigurationAcknowledged;
    
    private CurrentLimit? _currentLimit;
    private CurrentLimit? _currentLimitAck;
    private CurrentLimit? _currentLimitSent;
    
    internal MaxCharger(MaxModem modem)
    {
        _modem = modem;
    }

    public required byte Address { get; init; }
    public required string Serial { get; init; }
    public required string HardwareVersion { get; init; }
    public required string FirmwareVersion { get; init;}
    public string HardwareGeneration => HardwareVersion[^2..];

    public ChargerBoxState? ChargeBoxState { get; private set; }
    public ChargingMode? Mode { get; private set; }
    public MeterInfo? MeterInfo { get; private set; }
    public ChargerConfiguration? ChargerConfiguration { get; private set; }
    
    public void Initialize()
    {
        Send(MaxCommand.CONNECTION_STATE_CHANGED, new CONNECTION_STATE_CHANGED_REQUEST
        {
            // TODO: Does nothing? Actual interval is 16 minutes (960 secs).
            HeartbeatInterval = 900,
            LedEnable = 1
        });
    }

    void ITickable.Tick()
    {
        if (!ConfigureCharger())
        {
            return;
        }

        ConfigureCurrentLimit();
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
        if (Mode == null)
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
                Unknown_72 = 1000,
                Unknown_10 = "030000"u8.ToArray(),
                Unknown_18 = "01000100000000000000"u8.ToArray(),
                Unknown_40 = "000000003C0000"u8.ToArray(),
                Unknown_58 = "0000"u8.ToArray(),
                Unknown_64 = "01000000"u8.ToArray(),
                Unknown_76 = "010000"u8.ToArray()
            });
            return false;
        }

        return true;
    }

    private void ConfigureCurrentLimit()
    {
        // Update current limit.
        _currentLimit = Mode switch
        {
            ChargingMode.Ready => LimitLow,
            ChargingMode.Charging => LimitHigh,
            _ => _currentLimit
        };

        // Check if the (new) current limit has been acknowledged.
        var newLimit = _currentLimit;
        if (newLimit == null || _currentLimit == _currentLimitAck)
        {
            return;
        }

        // Check if the chargebox is in the correct state.
        if (ChargeBoxState != ChargerBoxState.ChargingCableConnected &&
            ChargeBoxState != ChargerBoxState.Charging &&
            ChargeBoxState != ChargerBoxState.Ready)
        {
            return;
        }
        
        _currentLimitSent = newLimit;
        
        Send(MaxCommand.SET_CURRENT_LIMIT, new SET_CURRENT_LIMIT_REQUEST
        {
            Unknown = 1,
            MinimumCurrent = newLimit.MinimumCurrent,
            CurrentLimitPhase1 = newLimit.Phase1,
            CurrentLimitPhase2 = newLimit.Phase2,
            CurrentLimitPhase3 = newLimit.Phase3
        });
    }
    
    internal void OnPacketReceived(MaxPacket packet)
    {
        if (packet.Command == MaxCommand.HEARTBEAT)
        {
            Send(MaxCommand.HEARTBEAT);
            return;
        }
        
        switch (packet.Data)
        {
            case CHARGING_STATE_REQUEST request:
            {
                Send(MaxCommand.CHARGING_STATE, new CHARGING_STATE_RESPONSE
                {
                    Ack = MaxAcknowledgment.ACK
                });
                
                Mode = (ChargingMode)request.State;

                // Clear the current limit to force reconfiguration.
                // TODO: Unable to get "Charging" mode, only from CB_STATE_UPDATE.
                if (Mode == ChargingMode.Ready || Mode == ChargingMode.Charging)
                {
                    _currentLimitAck = null;
                }
                
                Logger.Debug("Charging state updated: {State}", Mode);
                break;
            }
            case CB_STATE_UPDATE_REQUEST request:
            {
                Send(MaxCommand.CB_STATE_UPDATE, new CB_STATE_UPDATE_RESPONSE
                {
                    SessionId = request.SessionId,
                    Timestamp = MaxUtils.Timestamp()
                });
                
                ChargeBoxState = (ChargerBoxState)request.State;
                
                Logger.Debug("Charger box state updated: {State}", ChargeBoxState);
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
            case AUTHORIZE_CARD_REQUEST request:
            {
                Send(MaxCommand.AUTHORIZE_CARD, new AUTHORIZE_CARD_RESPONSE
                {
                    State = 0x01,
                    CardNumberLength = request.CardNumberLength,
                    CardNumberValue = request.CardNumberValue,
                    Unknown = 0xFFFF
                });
                break;
            }
            case SET_CURRENT_LIMIT_RESPONSE:
            {
                _currentLimitAck = _currentLimitSent;
                break;
            }
            case METERING_START_REQUEST meteringStartRequest:
            {
                // TODO: Handle metering start
                
                Send(MaxCommand.METERING_START, new METERING_START_RESPONSE
                {
                    State = 0x01,
                    SessionId = 7331, // TODO: Proper session logic
                    Timestamp = MaxUtils.Timestamp()
                });
                break;
            }
            case METERING_END_REQUEST meteringEndRequest:
            {
                // TODO: Handle metering end
                
                Send(MaxCommand.METERING_END, new METERING_END_RESPONSE
                {
                    State = 0x01
                });
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