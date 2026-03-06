using ChargePointNet.Core;
using ChargePointNet.Core.Protocols;

namespace ChargePointNet.Config;

public class DevicesConfig
{
    public const string Section = "Devices";

    public Dictionary<string, EVProtocol> Ports { get; set; } = [];
}