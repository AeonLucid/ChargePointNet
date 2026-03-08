using ChargePointNet.Core.Protocols.Max.Packets;
using ChargePointNet.Core.Protocols.Max.Packets.Data;

namespace ChargePointNet.Core.Protocols.Max;

public class MaxCharger : IChargeBox
{
    private readonly MaxModem _modem;
    private readonly MaxModemBus _bus;

    internal MaxCharger(byte address, MaxModem modem, MaxModemBus bus)
    {
        Address = address;
        _modem = modem;
        _bus = bus;
    }
    
    public byte Address { get; }

    public void Initialize()
    {
        Send(MaxCommand.CONNECTION_STATE_CHANGED, new CONNECTION_STATE_CHANGED_REQUEST
        {
            HeartbeatInterval = 900 * 256,
            LedEnable = 0
        });
    }

    internal void Send(MaxCommand command, IMaxPacketData data)
    {
        _modem.SendTo(Address, command, data);
    }

    public void Dispose()
    {
        
    }
}