using ChargePointNet.Core.Data;

namespace ChargePointNet.Core;

public interface IChargeBox : IDisposable
{
    bool Initialized { get; }
    string Serial { get; }
    string HardwareVersion { get; }
    string FirmwareVersion { get; }
    double ChassisTemperature { get; }
    double SocketTemperature { get; }
    ChargerBoxStatus Status { get; }
    ChargerMeter? Meter { get; }
    
    void UpdateLedBrightness(byte brightness);
}