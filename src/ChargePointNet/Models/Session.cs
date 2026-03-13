using ChargePointNet.Services.Sessions;

namespace ChargePointNet.Models;

public class Session
{
    public Session(ChargeSession y)
    {
        SessionId = y.Id;
        Serial = y.Key.Serial;
        CardNumber = y.Key.CardNumber;
        IsCharging = y.IsCharging;
        MeterValueStart = y.MeterValueStart;
        MeterValueCurrent = y.MeterValueCurrent;
        MeterValueEnd = y.MeterValueEnd;
        CreatedAt = y.CreatedAt;
        UpdatedAt = y.UpdatedAt;
        EndedAt = y.EndedAt;
    }

    /// <summary>
    ///     Unique identifier for the session.
    /// </summary>
    public Guid SessionId { get; init; }
    
    /// <summary>
    ///     Serial number of the charger.
    /// </summary>
    public string Serial { get; init; }
    
    /// <summary>
    ///     Card number of the NFC tag.
    /// </summary>
    public string CardNumber { get; init; }
    
    /// <summary>
    ///     Whether the charger is actively charging the EV.
    /// </summary>
    public bool IsCharging { get; init; }
    
    /// <summary>
    ///     Value of the meter in kWh at the start of the session.
    /// </summary>
    public double? MeterValueStart { get; init; }
    
    /// <summary>
    ///     Value of the meter in kWh at the current moment.
    /// </summary>
    public double? MeterValueCurrent { get; init; }
    
    /// <summary>
    ///     Value of the meter in kWh at the end of the session.
    /// </summary>
    public double? MeterValueEnd { get; init; }
    
    /// <summary>
    ///     Time when the session was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }
    
    /// <summary>
    ///     Time when the session was last updated.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; init; }
    
    /// <summary>
    ///     Time when the session ended. Null if the session is still active.
    /// </summary>
    public DateTimeOffset? EndedAt { get; init; }
}