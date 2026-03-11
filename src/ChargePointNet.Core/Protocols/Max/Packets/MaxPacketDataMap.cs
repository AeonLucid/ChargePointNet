using ChargePointNet.Packets.Max;

namespace ChargePointNet.Core.Protocols.Max.Packets;

internal static class MaxPacketDataMap
{
    public static Type? GetType(byte destination, byte source, MaxCommand command)
    {
        return command switch
        {
            MaxCommand.CB_REGISTER => destination == MaxAddress.MODEM
                ? typeof(CB_REGISTER_REQUEST)
                : typeof(CB_REGISTER_RESPONSE),
            MaxCommand.GET_METER_INFO => source == MaxAddress.MODEM
                ? typeof(GET_METER_INFO_REQUEST)
                : typeof(GET_METER_INFO_RESPONSE),
            MaxCommand.AUTHORIZE_CARD => destination == MaxAddress.MODEM
                ? typeof(AUTHORIZE_CARD_REQUEST)
                : typeof(AUTHORIZE_CARD_RESPONSE),
            MaxCommand.METERING_START => destination == MaxAddress.MODEM
                ? typeof(METERING_START_REQUEST)
                : typeof(METERING_START_RESPONSE),
            MaxCommand.METERING_END => destination == MaxAddress.MODEM
                ? typeof(METERING_END_REQUEST)
                : typeof(METERING_END_RESPONSE),
            MaxCommand.CB_STATE_UPDATE => destination == MaxAddress.MODEM
                ? typeof(CB_STATE_UPDATE_REQUEST)
                : typeof(CB_STATE_UPDATE_RESPONSE),
            MaxCommand.GET_CB_CONFIGURATION => source == MaxAddress.MODEM
                ? typeof(GET_CB_CONFIGURATION_REQUEST)
                : typeof(GET_CB_CONFIGURATION_RESPONSE),
            MaxCommand.SET_CB_CONFIGURATION => source == MaxAddress.MODEM
                ? typeof(SET_CB_CONFIGURATION_REQUEST)
                : typeof(SET_CB_CONFIGURATION_RESPONSE),
            MaxCommand.CHARGING_STATE => destination == MaxAddress.MODEM
                ? typeof(CHARGING_STATE_REQUEST)
                : typeof(CHARGING_STATE_RESPONSE),
            MaxCommand.SET_CURRENT_LIMIT => source == MaxAddress.MODEM
                ? typeof(SET_CURRENT_LIMIT_REQUEST)
                : typeof(SET_CURRENT_LIMIT_RESPONSE),
            _ => typeof(UNKNOWN)
        };
    }
}