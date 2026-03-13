namespace ChargePointNet.Models;

public class Session
{
    /// <summary>
    ///     Unique identifier for the session.
    /// </summary>
    public required Guid SessionId { get; init; }
    
    /// <summary>
    ///     Serial number of the charger.
    /// </summary>
    public required string Serial { get; set; }
    
    /// <summary>
    ///     Card number of the NFC tag.
    /// </summary>
    public required string CardNumber { get; set; }
    
    /// <summary>
    ///     Whether the charger is actively charging the EV.
    /// </summary>
    public required bool IsCharging { get; set; }
    
    /// <summary>
    ///     Value of the meter in kWh at the start of the session.
    /// </summary>
    public required double? MeterValueStart { get; set; }
    
    /// <summary>
    ///     Value of the meter in kWh at the current moment.
    /// </summary>
    public required double? MeterValueCurrent { get; set; }
    
    /// <summary>
    ///     Value of the meter in kWh at the end of the session.
    /// </summary>
    public required double? MeterValueEnd { get; set; }
    
    /// <summary>
    ///     Time when the session was created.
    /// </summary>
    public required DateTimeOffset CreatedAt { get; set; }
    
    /// <summary>
    ///     Time when the session was last updated.
    /// </summary>
    public required DateTimeOffset UpdatedAt { get; set; }
    
    /// <summary>
    ///     Time when the session ended. Null if the session is still active.
    /// </summary>
    public required DateTimeOffset? EndedAt { get; set; }
}