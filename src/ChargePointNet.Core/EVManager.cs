using System.IO.Ports;
using ChargePointNet.Core.Protocols;
using ChargePointNet.Core.Protocols.Max;
using Serilog;

namespace ChargePointNet.Core;

public class EVManager
{
    private static readonly ILogger Logger = Log.ForContext<EVManager>();
    
    private readonly Dictionary<string, IModem> _modems;
    private readonly Dictionary<string, IChargeBox> _boxes;

    public EVManager()
    {
        _modems = [];
        _boxes = [];
    }

    public bool IsRegistered(string portName)
    {
        return _modems.ContainsKey(portName) || _boxes.ContainsKey(portName);
    }
    
    public void RegisterDevice(SerialPort port, EVProtocol protocol)
    {
        switch (protocol)
        {
            case EVProtocol.Max:
                _modems[port.PortName] = new MaxModem(port);
                _modems[port.PortName].Start();
                break;
            default:
                throw new NotSupportedException($"Protocol {protocol} is not supported");
        }
    }

    public void Cleanup()
    {
        foreach (var modem in _modems)
        {
            if (!modem.Value.Connected)
            {
                Logger.Warning("Lost connection with modem on port {ModemPort}", modem.Key);
                
                modem.Value.Dispose();
                
                _modems.Remove(modem.Key);
            }
        }
    }
}