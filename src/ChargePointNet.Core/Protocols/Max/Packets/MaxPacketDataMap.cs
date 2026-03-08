using ChargePointNet.Core.Protocols.Max.Packets.Data;

namespace ChargePointNet.Core.Protocols.Max.Packets;

internal static class MaxPacketDataMap
{
    public static Type GetType(byte destination, byte source, MaxCommand command)
    {
        switch (command)
        {
            case MaxCommand.CB_REGISTER:
                return destination == 0x80 ? typeof(CB_REGISTER_REQUEST) : typeof(CB_REGISTER_RESPONSE);
            default:
                return typeof(UNKNOWN);
        }
    }
}