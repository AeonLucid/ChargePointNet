using ChargePointNet.Core.Protocols;

namespace ChargePointNet.Config;

public class DevicesConfig
{
    public const string Section = "Devices";

    public List<SerialDevice> Serial { get; set; } = [];
    public List<NetworkDevice> Network { get; set; } = [];

    public class SerialDevice
    {
        public required bool Enabled { get; set; }
        public required string Port { get; set; }
        public required EVProtocol Protocol { get; set; }
    }
    
    public class NetworkDevice
    {
        public required bool Enabled { get; set; }
        public required string IpAddress { get; set; }
        public required int Port { get; set; }
        public required EVProtocol Protocol { get; set; }

        public override string ToString()
        {
            return $"{IpAddress}:{Port}";
        }
    }
}