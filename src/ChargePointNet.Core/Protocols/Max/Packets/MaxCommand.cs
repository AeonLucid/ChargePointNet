namespace ChargePointNet.Core.Protocols.Max.Packets;

internal enum MaxCommand : byte
{
    CB_REGISTER = 0x11,
    CONNECTION_STATE_CHANGED = 0x1B,
    RESTART_REGISTRATION = 0x1E,
}