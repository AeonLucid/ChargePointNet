using ChargePointNet.Core.Protocols.Max.Packets.Data;

namespace ChargePointNet.Core.Protocols.Max.Packets;

internal static class MaxPacketDataMap
{
    public static Type GetType(byte destination, byte source, MaxCommand command)
    {
        switch (command)
        {
            case MaxCommand.CB_REGISTER:
                return destination == MaxAddress.MODEM ? typeof(CB_REGISTER_REQUEST) : typeof(CB_REGISTER_RESPONSE);
            case MaxCommand.CB_STATE_UPDATE:
                return destination == MaxAddress.MODEM ? typeof(CB_STATE_UPDATE_REQUEST) : typeof(CB_STATE_UPDATE_RESPONSE);
            case MaxCommand.CHARGING_STATE:
                return destination == MaxAddress.MODEM ? typeof(CHARGING_STATE_REQUEST) : typeof(CHARGING_STATE_RESPONSE);
            default:
                return typeof(UNKNOWN);
        }
    }
}