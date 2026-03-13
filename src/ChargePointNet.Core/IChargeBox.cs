using ChargePointNet.Core.Data;
using ChargePointNet.Core.Interfaces;

namespace ChargePointNet.Core;

public interface IChargeBox : IDisposable
{
    bool Initialized { get; }
    
    /// <summary>
    ///     Serial number of the charger.
    /// </summary>
    string Serial { get; }
    
    /// <summary>
    ///     Hardware version of the charger.
    /// </summary>
    string HardwareVersion { get; }
    
    /// <summary>
    ///     Firmware version of the charger.
    /// </summary>
    string FirmwareVersion { get; }
    
    /// <summary>
    ///     Chassis temperature of the charger in Celsius.
    /// </summary>
    double ChassisTemperature { get; }
    
    /// <summary>
    ///     Socket temperature of the charger in Celsius.
    /// </summary>
    double SocketTemperature { get; }
    
    /// <summary>
    ///     Whether the charger is actively charging the EV.
    /// </summary>
    bool IsCharging { get; }
    
    /// <summary>
    ///     Whether the charger is locked (i.e., the cable is locked in place and cannot be removed).
    /// </summary>
    bool IsLocked { get; }
    
    /// <summary>
    ///     Whether the charger is configured to automatically start charging when a car is plugged in.
    /// </summary>
    bool AutoStart { get; }
    
    /// <summary>
    ///     Whether the charger is configured to allow a remote start of charging sessions.
    /// </summary>
    bool RemoteStart { get; }
    
    /// <summary>
    ///     Current LED brightness of the charger (0-100)%.
    /// </summary>
    double LedBrightness { get; }
    
    /// <summary>
    ///      Current LED color of the charger.
    /// </summary>
    LedColor LedColor { get; }
    
    /// <summary>
    ///     Status of the charger.
    /// </summary>
    ChargerBoxStatus Status { get; }
    
    ChargerMeter? Meter { get; }
    
    /// <summary>
    ///     Current charging session, if any. Null if no session is active.
    /// </summary>
    IChargeSession? CurrentSession { get; }
    
    /// <summary>
    ///     The timestamp when the charger was connected.
    /// </summary>
    DateTimeOffset CreatedAt { get; }
    
    /// <summary>
    ///     The timestamp when the charger was last updated.
    /// </summary>
    DateTimeOffset UpdatedAt { get; }
    
    void UpdateAutostart(bool enable);
    void UpdateLedBrightness(byte brightness);
}