using ChargePointNet.Core.Data;
using ChargePointNet.Core.Interfaces;
using ChargePointNet.Core.Protocols.Max.Data;
using ChargePointNet.Core.Protocols.Max.Packets;
using ChargePointNet.Packets;
using ChargePointNet.Packets.Max;
using Serilog;

namespace ChargePointNet.Core.Protocols.Max;

public class MaxCharger : IChargeBox, ITickable
{
    private static readonly ILogger Logger = Log.ForContext<MaxCharger>();
    
    /// <summary>
    ///     EVBox keeps polling for a minute.
    /// </summary>
    private static readonly TimeSpan AuthTimeout = TimeSpan.FromSeconds(60);
    
    private static readonly CurrentLimit LimitLow = new(60, 60, 60, 60);
    private static readonly CurrentLimit LimitHigh = new(60, 160, 160, 160);

    private const string EmptySerial = "0000000000000000";
    
    private readonly MaxModem _modem;
    private readonly IAuthRepository _authRepository;
    private readonly ISessionRepository _sessionRepository;

    // TODO: What can we map meter type to? Firmware? Hardware version?
    
    private bool _isConfigurationAcknowledged;
    private bool _isCharging;
    private bool _isLocked;
    
    private CurrentLimit? _currentLimit;
    private CurrentLimit? _currentLimitAck;
    private CurrentLimit? _currentLimitSent;

    private ChargerBoxState? _chargerBoxState;
    private ChargingMode? _mode;
    private MaxMeterInfo? _meterInfo;
    private ChargerConfiguration? _chargerConfiguration;
    private LedColour _ledColour = LedColour.Off;
    
    internal MaxCharger(MaxModem modem, IAuthRepository authRepository, ISessionRepository sessionRepository)
    {
        _modem = modem;
        _authRepository = authRepository;
        _sessionRepository = sessionRepository;
    }

    public bool Initialized { get; private set; }

    public required byte Address { get; init; }
    public required string Serial { get; init; }
    public required string HardwareVersion { get; init; }
    public required string FirmwareVersion { get; init;}
    public string HardwareGeneration => HardwareVersion[^2..];
    public double ChassisTemperature { get; private set; }
    public double SocketTemperature { get; private set; }

    private uint CurrentSessionId => (uint?)CurrentSession?.Id.GetHashCode() ?? 0;
    public IChargeSession? CurrentSession { get; private set; }

    public ChargerBoxStatus Status => _chargerBoxState switch
    {
        ChargerBoxState.Available => ChargerBoxStatus.Available,
        ChargerBoxState.Error => ChargerBoxStatus.Error,
        ChargerBoxState.ChargingCableConnected => ChargerBoxStatus.Available,
        ChargerBoxState.Charging => ChargerBoxStatus.Charging,
        ChargerBoxState.Ready => ChargerBoxStatus.Ready,
        ChargerBoxState.Finished => ChargerBoxStatus.Available, // TODO: When triggered?
        null => ChargerBoxStatus.Unknown,
        _ => throw new ArgumentOutOfRangeException()
    };

    public ChargerMeter? Meter => _meterInfo != null
        ? new ChargerMeter(
            _chargerConfiguration!.MeterType == 0 ? ChargerMeterType.Serial : ChargerMeterType.Pulse,
            _meterInfo.Version,
            _meterInfo.Model,
            _meterInfo.Serial,
            _meterInfo.MainsFrequency)
        : null;

    public void Initialize()
    {
        Send(MaxCommand.CONNECTION_STATE_CHANGED, new CONNECTION_STATE_CHANGED_REQUEST
        {
            // TODO: Does nothing? Actual interval is 16 minutes (960 secs).
            HeartbeatInterval = 120,
            LedEnable = 1
        });
    }

    public void UpdateLedBrightness(byte brightness)
    {
        if (brightness > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(brightness), "Brightness must be between 0 and 100.");
        }

        if (_chargerConfiguration != null)
        {
            _chargerConfiguration.LedBrightness = brightness;
            _isConfigurationAcknowledged = false;
        }
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
        if (_mode == null)
        {
            return false;
        }
        
        // Fetch meter info.
        if (_meterInfo == null)
        {
            Send(MaxCommand.GET_METER_INFO);
            return false;
        }
        
        // Fetch charger configuration.
        if (_chargerConfiguration == null)
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
                LedBrightness = _chargerConfiguration.LedBrightness,
                MeterType = (byte)_chargerConfiguration!.MeterType,
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

        Initialized = true;
        return true;
    }

    private void ConfigureCurrentLimit()
    {
        // Update current limit.
        _currentLimit = _mode switch
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
        if (_chargerBoxState != ChargerBoxState.ChargingCableConnected &&
            _chargerBoxState != ChargerBoxState.Charging &&
            _chargerBoxState != ChargerBoxState.Ready)
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

                var previousMode = _mode;
                
                _mode = (ChargingMode)request.State;

                // Clear the current limit to force reconfiguration.
                // TODO: Unable to get "Charging" mode, only from CB_STATE_UPDATE.
                //  (_mode == ChargingMode.Ready || _mode == ChargingMode.Charging)
                if (previousMode == ChargingMode.Available && _mode == ChargingMode.Ready)
                {
                    _currentLimitAck = null;
                }
                
                // Logger.Debug("Charging state updated: {State}", Mode);
                break;
            }
            case CB_STATE_UPDATE_REQUEST request:
            {
                Send(MaxCommand.CB_STATE_UPDATE, new CB_STATE_UPDATE_RESPONSE
                {
                    SessionId = CurrentSessionId,
                    Timestamp = MaxUtils.Timestamp()
                });
                
                _chargerBoxState = (ChargerBoxState)request.State;
                _ledColour = (LedColour)request.LedColour;
                _isCharging = request.IsCharging == 1;
                _isLocked = request.IsLocked == 1;
                ChassisTemperature = request.ChassisTemperature / 10d;
                SocketTemperature = request.SocketTemperature / 10d;

                if (CurrentSession != null && CurrentSessionId == request.SessionId)
                {
                    CurrentSession.IsCharging = _isCharging;
                    CurrentSession.MeterValueCurrent = request.MeterValue / 1000d;
                    CurrentSession.UpdatedAt = DateTimeOffset.UtcNow;
                }
                
                Logger.Debug("Charger state: {State}, mode: {Mode}, color: {Colour}", _chargerBoxState, _mode, _ledColour);
                break;
            }
            case GET_METER_INFO_RESPONSE response:
            {
                _meterInfo = new MaxMeterInfo
                {
                    Version = response.VersionNumberLength != 0
                        ? response.VersionNumber![..response.VersionNumberLength]
                        : null,
                    Model = response.ModelNameLength != 0
                        ? response.ModelName![..response.ModelNameLength]
                        : null,
                    Serial = response.SerialNumber != EmptySerial
                        ? response.SerialNumber
                        : null,
                    MainsFrequency = response.MainsFrequency != 0
                        ? response.MainsFrequency / 100d
                        : null,
                };
                break;
            }
            case GET_CB_CONFIGURATION_RESPONSE response:
            {
                _chargerConfiguration = new ChargerConfiguration
                {
                    MeterUpdateInterval = response.MeterUpdateInterval,
                    MeterType = (MaxMeterType)response.MeterType,
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
                var key = new AuthorizationContext(Serial, request.CardNumberValue![..request.CardNumberLength]);
                var pending = _authRepository.GetOrCreate(key, AuthTimeout);
                if (pending.IsPending)
                {
                    return;
                }
                
                _authRepository.Remove(key);
                
                Send(MaxCommand.AUTHORIZE_CARD, new AUTHORIZE_CARD_RESPONSE
                {
                    State = (byte)(pending.IsAuthorized ? 0x01 : 0x12),
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
            case METERING_START_REQUEST request:
            {
                var key = new AuthorizationContext(Serial, request.CardNumberValue![..request.CardNumberLength]);
                
                CurrentSession = _sessionRepository.CreateSession(key);
                CurrentSession.MeterValueStart = request.MeterValue / 1000d;
                
                Logger.Information("Session {SessionId} started. Meter value: {MeterValue} kWh", CurrentSession.Id, request.MeterValue / 1000d);
                
                Send(MaxCommand.METERING_START, new METERING_START_RESPONSE
                {
                    State = 0x01,
                    SessionId = CurrentSessionId,
                    Timestamp = MaxUtils.Timestamp(CurrentSession.CreatedAt)
                });
                break;
            }
            case METERING_END_REQUEST request:
            {
                if (CurrentSession != null && CurrentSessionId == request.SessionId)
                {
                    Logger.Information("Session {SessionId} ended. Meter value: {MeterValue} kWh", CurrentSession.Id, request.MeterValue / 1000d);
                    
                    CurrentSession.IsCharging = false;
                    CurrentSession.MeterValueEnd = request.MeterValue / 1000d;
                    CurrentSession.UpdatedAt = DateTimeOffset.UtcNow;
                    CurrentSession.EndedAt = CurrentSession.UpdatedAt;
                    CurrentSession = null;
                }
                else
                {
                    Logger.Warning("Received METERING_END_REQUEST for unknown session {SessionId}", request.SessionId);
                }
                
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