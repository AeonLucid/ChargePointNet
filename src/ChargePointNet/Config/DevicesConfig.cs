using ChargePointNet.Core.Protocols;

namespace ChargePointNet.Config;

public class DevicesConfig
{
    public const string Section = "Devices";

    public Dictionary<string, EVProtocol> Ports { get; set; } = [];
    public List<DeviceEndpoint> Endpoints { get; set; } = [];

    public class DeviceEndpoint
    {
        public required string IpAddress { get; set; }
        public required int Port { get; set; }
        public required EVProtocol Protocol { get; set; }

        public override string ToString()
        {
            return $"{IpAddress}:{Port}";
        }
    }
}