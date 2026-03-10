using ChargePointNet.Core.Protocols.Max.Packets;
using ChargePointNet.Packets;
using ChargePointNet.Packets.Max;

namespace ChargePointNet.Core.Protocols.Max;

public class MaxCharger : IChargeBox
{
    private readonly MaxModem _modem;

    internal MaxCharger(MaxModem modem)
    {
        _modem = modem;
    }

    public required byte Address { get; init; }
    public required string Serial { get; init; }
    public required string HardwareVersion { get; init; }
    public required string FirmwareVersion { get; init;}
    
    public string HardwareGeneration => HardwareVersion[^2..];

    public void Initialize()
    {
        Send(MaxCommand.CONNECTION_STATE_CHANGED, new CONNECTION_STATE_CHANGED_REQUEST
        {
            HeartbeatInterval = 900 * 256,
            LedEnable = 1
        });
    }

    internal void Send(MaxCommand command, IHexPacket? data = null)
    {
        _modem.SendTo(Address, command, data);
    }

    internal void OnPacketReceived(MaxPacket packet)
    {
        switch (packet.Data)
        {
            case CHARGING_STATE_REQUEST chargingStateRequest:
                Send(MaxCommand.CHARGING_STATE, new CHARGING_STATE_RESPONSE
                {
                    Ack = 0
                });
                break;
            case CB_STATE_UPDATE_REQUEST stateUpdateRequest:
                Send(MaxCommand.CB_STATE_UPDATE, new CB_STATE_UPDATE_RESPONSE
                {
                    SessionId = 0,
                    Timestamp = MaxUtils.Timestamp()
                });
            
                Send(MaxCommand.GET_METER_INFO);
                // TODO: Get meter info
                break;
        }
    }

    public void Dispose()
    {
        
    }
}