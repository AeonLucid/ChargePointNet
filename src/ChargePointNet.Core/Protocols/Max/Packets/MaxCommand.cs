namespace ChargePointNet.Core.Protocols.Max.Packets;

/// <summary>
///     11–1F are initialization,
///     21–2F are initiated by the CB,
///     31–3F are initiated by the CP,
///     41–4F are hardware configuration,
///     61–6F are power management
///     E1–FF mostly deal with ChargeStation communication.
/// </summary>
internal enum MaxCommand : byte
{
    CB_REGISTER = 0x11,
    GET_METER_INFO = 0x13,
    CONNECTION_STATE_CHANGED = 0x1B,
    RESTART_REGISTRATION = 0x1E,
    HEARTBEAT = 0x21,
    AUTHORIZE_CARD = 0x22,
    METERING_START = 0x23,
    METERING_END = 0x24,
    CB_STATE_UPDATE = 0x26,
    GET_CB_CONFIGURATION = 0x33,
    SET_CB_CONFIGURATION = 0x34,
    CHARGING_STATE = 0x6A,
    SET_CURRENT_LIMIT = 0x6B,
}