namespace ChargePointNet.Core.Data;

/// <summary>
///     Test
/// </summary>
public enum ChargerBoxStatus
{
    /// <summary>
    ///     Status is unknown.
    /// </summary>
    Unknown,
    /// <summary>
    ///     Charger is available, waiting for an action.
    /// </summary>
    Available,
    /// <summary>
    ///     Charger is waiting to charge.
    /// </summary>
    Ready,
    /// <summary>
    ///     EV is charging.
    /// </summary>
    Charging,
    /// <summary>
    ///     Something went wrong inside the charger.
    /// </summary>
    Error
}