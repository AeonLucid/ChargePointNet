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
    
    private readonly MaxModem _modem;
    private readonly IAuthRepository _authRepository;
    private readonly ISessionRepository _sessionRepository;

    // TODO: What can we map meter type to? Firmware? Hardware version?
    
    private bool _isConfigurationAcknowledged;
    private byte _ledColor;
    
    private CurrentLimit? _currentLimit;
    private CurrentLimit? _currentLimitAck;
    private CurrentLimit? _currentLimitSent;

    private ChargerBoxState? _chargerBoxState;
    private ChargingMode? _mode;
    private MaxMeterInfo? _meterInfo;
    private ChargerConfiguration? _chargerConfiguration;

    internal MaxCharger(MaxModem modem, IAuthRepository authRepository, ISessionRepository sessionRepository)
    {
        _modem = modem;
        _authRepository = authRepository;
        _sessionRepository = sessionRepository;

        Phases = [];
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public bool Initialized { get; private set; }

    public required byte Address { get; init; }
    public required string Serial { get; init; }
    public required string HardwareVersion { get; init; }
    public required string FirmwareVersion { get; init;}
    public string HardwareGeneration => HardwareVersion[^2..];
    public double ChassisTemperature { get; private set; }
    public double SocketTemperature { get; private set; }
    public bool IsCharging { get; private set; }
    public bool IsLocked { get; private set; }
    public bool AutoStart => _chargerConfiguration?.AutoStart == 1;
    public bool RemoteStart => _chargerConfiguration?.RemoteStart == 1;

    private uint CurrentSessionId => (uint?)CurrentSession?.Id.GetHashCode() ?? 0;
    public IChargeSession? CurrentSession { get; private set; }
    
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public double LedBrightness => _chargerConfiguration?.LedBrightness ?? 0;
    
    public LedColor LedColor => _ledColor switch {
        0 => LedColor.Unknown,
        1 => LedColor.Green,
        2 => LedColor.Red,
        3 => LedColor.Yellow,
        4 => LedColor.Blue,
        _ => LedColor.Unknown
    };

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

    public Phase[] Phases { get; private set; }
    
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
        Logger.Information("Initializing charger {Serial}", Serial);
        
        Send(MaxCommand.CONNECTION_STATE_CHANGED, new CONNECTION_STATE_CHANGED_REQUEST
        {
            // TODO: Does nothing? Actual interval is 16 minutes (960 secs).
            HeartbeatInterval = 900,
            LedEnable = 1
        });
    }

    public void UpdateAutostart(bool enable)
    {
        if (_chargerConfiguration != null)
        {
            _chargerConfiguration.AutoStart = (byte)(enable ? 1 : 0);
            _isConfigurationAcknowledged = false;
        }
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
                AutoStart = _chargerConfiguration.AutoStart,
                Unknown_54 = 900,
                MeterUpdateInterval = _chargerConfiguration.MeterUpdateInterval,
                RemoteStart = _chargerConfiguration.RemoteStart,
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

        if (!Initialized)
        {
            Logger.Information("Charger initialized. " +
                               "Serial: {Serial}, " +
                               "Hardware: {HardwareVersion}, " +
                               "Firmware: {FirmwareVersion}, " +
                               "Meter type: {MeterType}", Serial, HardwareVersion, FirmwareVersion, _chargerConfiguration.MeterType);
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

        if (Phases.Length == 3)
        {
            Phases[0].CurrentLimit = newLimit.Phase1 / 10d;
            Phases[1].CurrentLimit = newLimit.Phase2 / 10d;
            Phases[2].CurrentLimit = newLimit.Phase3 / 10d;
        }
        
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
            Logger.Debug("Received heartbeat from {Source}", packet.Source);
            
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
                
                Logger.Debug("Charging mode updated: {State}", _mode);
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
                _ledColor = request.LedColor;
                IsCharging = request.IsCharging == 1;
                IsLocked = request.IsLocked == 1;
                ChassisTemperature = request.ChassisTemperature / 10d;
                SocketTemperature = request.SocketTemperature / 10d;

                if (CurrentSession != null && CurrentSessionId == request.SessionId)
                {
                    if (CurrentSession.IsCharging != IsCharging)
                    {
                        Logger.Information("Session {SessionId} is now {State}", CurrentSession.Id, IsCharging ? "charging" : "not charging");
                    }
                    
                    CurrentSession.IsCharging = IsCharging;
                    CurrentSession.MeterValueCurrent = request.MeterValue / 1000d;
                    CurrentSession.UpdatedAt = DateTimeOffset.UtcNow;
                }

                if (Phases.Length == 0)
                {
                    Phases = [
                        new Phase(request.VoltagePhase1, request.CurrentPhase1 / 100d, request.PowerFactorPhase1 / 1000d),
                        new Phase(request.VoltagePhase2, request.CurrentPhase2 / 100d, request.PowerFactorPhase2 / 1000d),
                        new Phase(request.VoltagePhase3, request.CurrentPhase3 / 100d, request.PowerFactorPhase3 / 1000d),
                    ];
                }
                else
                {
                    Phases[0].Voltage = request.VoltagePhase1;
                    Phases[0].Current = request.CurrentPhase1 / 100d;
                    Phases[0].PowerFactor = request.PowerFactorPhase1 / 1000d;
                    
                    Phases[1].Voltage = request.VoltagePhase2;
                    Phases[1].Current = request.CurrentPhase2 / 100d;
                    Phases[1].PowerFactor = request.PowerFactorPhase2 / 1000d;
                    
                    Phases[2].Voltage = request.VoltagePhase3;
                    Phases[2].Current = request.CurrentPhase3 / 100d;
                    Phases[2].PowerFactor = request.PowerFactorPhase3 / 1000d;
                }
                
                UpdatedAt = DateTimeOffset.UtcNow;
                
                Logger.Debug("Charger state: {State}, color: {Color}", _chargerBoxState, _ledColor);
                break;
            }
            case GET_METER_INFO_RESPONSE response:
            {
                Logger.Information("Serial {Serial}: received meter info", Serial);
                
                _meterInfo = new MaxMeterInfo
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
                Logger.Information("Serial {Serial}: received configuration", Serial);
                
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
                Logger.Information("Serial {Serial}: updated configuration", Serial);
                
                _isConfigurationAcknowledged = response.Ack == MaxAcknowledgment.ACK;
                break;
            }
            case AUTHORIZE_CARD_REQUEST request:
            {
                Logger.Information("Requesting authorization for card {CardNumber}", request.CardNumberValue);
                
                var key = new AuthorizationContext(Serial, request.CardNumberValue![..request.CardNumberLength]);
                var pending = _authRepository.GetOrCreate(key, AuthTimeout);
                if (pending.IsPending)
                {
                    return;
                }
                
                _authRepository.Remove(key);
                
                Logger.Information("Card {CardNumber} has been {Value}", request.CardNumberValue, pending.IsAuthorized ? "authorized" : "unauthorized");
                
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