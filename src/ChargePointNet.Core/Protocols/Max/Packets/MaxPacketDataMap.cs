using ChargePointNet.Packets.Max;

namespace ChargePointNet.Core.Protocols.Max.Packets;

internal static class MaxPacketDataMap
{
    public static Type GetType(byte destination, byte source, MaxCommand command)
    {
        return command switch
        {
            MaxCommand.CB_REGISTER => destination == MaxAddress.MODEM
                ? typeof(CB_REGISTER_REQUEST)
                : typeof(CB_REGISTER_RESPONSE),
            MaxCommand.CB_STATE_UPDATE => destination == MaxAddress.MODEM
                ? typeof(CB_STATE_UPDATE_REQUEST)
                : typeof(CB_STATE_UPDATE_RESPONSE),
            MaxCommand.CHARGING_STATE => destination == MaxAddress.MODEM
                ? typeof(CHARGING_STATE_REQUEST)
                : typeof(CHARGING_STATE_RESPONSE),
            _ => typeof(UNKNOWN)
        };
    }
}