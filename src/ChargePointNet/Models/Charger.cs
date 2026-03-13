using System.Text.Json.Serialization;
using ChargePointNet.Core.Data;
using ChargePointNet.Core.Interfaces;

namespace ChargePointNet.Models;

public class Charger
{
    public Charger(IChargeBox box)
    {
        Serial = box.Serial;
        Status = box.Status;
        HardwareVersion = box.HardwareVersion;
        FirmwareVersion = box.FirmwareVersion;
        ChassisTemperature = box.ChassisTemperature;
        SocketTemperature = box.SocketTemperature;
        IsCharging = box.IsCharging;
        IsLocked = box.IsLocked;
        AutoStart = box.AutoStart;
        RemoteStart = box.RemoteStart;
        LedBrightness = box.LedBrightness;
        LedColor = box.LedColor;
        CurrentSessionId = box.CurrentSession?.Id;
        Phases = box.Phases;
        Meter = box.Meter != null ? new MeterInfo(box.Meter) : null;
        CreatedAt = box.CreatedAt;
        UpdatedAt = box.UpdatedAt;
    }

    /// <summary>
    ///     <inheritdoc cref="IChargeBox.Serial"/>
    /// </summary>
    public string Serial { get; init; }
    
    /// <summary>
    ///     <inheritdoc cref="IChargeBox.Status"/>
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ChargerBoxStatus Status { get; init; }
    
    /// <summary>
    ///     <inheritdoc cref="IChargeBox.HardwareVersion"/>
    /// </summary>
    public string HardwareVersion { get; init; }
    
    /// <summary>
    ///     <inheritdoc cref="IChargeBox.FirmwareVersion"/>
    /// </summary>
    public string FirmwareVersion { get; init; }
    
    /// <summary>
    ///     <inheritdoc cref="IChargeBox.ChassisTemperature"/>
    /// </summary>
    public double ChassisTemperature { get; init; }
    
    /// <summary>
    ///     <inheritdoc cref="IChargeBox.SocketTemperature"/>
    /// </summary>
    public double SocketTemperature { get; init; }
    
    /// <summary>
    ///     <inheritdoc cref="IChargeBox.IsCharging"/>
    /// </summary>
    public bool IsCharging { get; init; }
    
    /// <summary>
    ///     <inheritdoc cref="IChargeBox.IsLocked"/>
    /// </summary>
    public bool IsLocked { get; init; }
    
    /// <summary>
    ///     <inheritdoc cref="IChargeBox.AutoStart"/>
    /// </summary>
    public bool AutoStart { get; init; }
    
    /// <summary>
    ///     <inheritdoc cref="IChargeBox.RemoteStart"/>
    /// </summary>
    public bool RemoteStart { get; init; }
    
    /// <summary>
    ///     <inheritdoc cref="IChargeBox.LedBrightness"/>
    /// </summary>
    public double LedBrightness { get; init; }
    
    /// <summary>
    ///     <inheritdoc cref="IChargeBox.LedColor"/>
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public LedColor LedColor { get; init; }
    
    /// <summary>
    ///     The ID of the current <see cref="Session"/>, if any. Null if no session is active.
    /// </summary>
    public Guid? CurrentSessionId { get; init; }
    
    /// <summary>
    ///     <inheritdoc cref="IChargeBox.Phases"/>
    /// </summary>
    public Phase[] Phases { get; init; }
    
    /// <summary>
    ///     Meter information of the charger, if available.
    ///     This may be null if the charger does not have a meter or if the meter information is not available.
    /// </summary>
    public MeterInfo? Meter { get; init; }
    
    /// <summary>
    ///     <inheritdoc cref="IChargeBox.CreatedAt"/>
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }
    
    /// <summary>
    ///     <inheritdoc cref="IChargeBox.UpdatedAt"/>
    /// </summary>
    public DateTimeOffset UpdatedAt { get; init; }
}