namespace ChargePointNet.Core.Protocols.Max.Data;

/// <summary>
///     Proper order
///     A0 (available) → A7 (ready) → 81 (charging) → C1 (finished) → 80 (unplugged) → A0 (available)
/// </summary>
internal enum ChargingState : byte
{
    Unknown_07 = 0x07,
    Unknown_20 = 0x20,
    Unknown_28 = 0x28,
    Unknown_2F = 0x2F,
    Unplugged = 0x80,
    Charging = 0x81,
    Available = 0xA0,
    Ready = 0xA7,
    Finished = 0xC1,
    Failed = 0xE7
}