namespace ChargePointNet.Core.Protocols.Max;

internal static class MaxAddress
{
    public const byte NEW = 0x00;
    public const byte CB_MIN = 0x01;
    public const byte CB_MAX = 0x01 + 20 - 1;
    public const byte MODEM = 0x80;
    public const byte BROADCAST = 0xBC;
    public const byte CHARGE_STATION = 0xFD;
}