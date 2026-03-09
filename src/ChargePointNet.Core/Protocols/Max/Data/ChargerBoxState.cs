namespace ChargePointNet.Core.Protocols.Max.Data;

/// <summary>
///     02 (available) → 47 (charging cable connected) → 4A (ready) → 48 (charging) → 4A (ready) → 4B (finished) → 02 (available)
/// </summary>
internal enum ChargerBoxState : byte
{
    Available = 0x02,
    Error = 0x0A,
    ChargingCableConnected = 0x47,
    Charging = 0x48,
    Ready = 0x4A,
    /// <summary>
    ///     EV still plugged in.
    /// </summary>
    Finished = 0x4B,
}