using System.IO.Ports;
using ChargePointNet.Config;
using ChargePointNet.Core;
using Microsoft.Extensions.Options;

namespace ChargePointNet.Services;

public class DeviceRegistrationService : BackgroundService
{
    private readonly ILogger<DeviceRegistrationService> _logger;
    private readonly IOptionsMonitor<DevicesConfig> _devicesConfig;
    private readonly PeriodicTimer _periodicTimer;
    private readonly EVManager _evManager;

    public DeviceRegistrationService(ILogger<DeviceRegistrationService> logger, IOptionsMonitor<DevicesConfig> devicesConfig, EVManager evManager)
    {
        _logger = logger;
        _devicesConfig = devicesConfig;
        _periodicTimer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        _evManager = evManager;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DeviceDetectionService running");

        do
        {
            // Clean up.
            _evManager.Cleanup();
            
            // Register new devices.
            var config = _devicesConfig.CurrentValue;

            foreach (var (portName, protocol) in config.Ports)
            {
                // Check if the port is already used.
                if (_evManager.IsRegistered(portName))
                {
                    continue;
                }
                
                // Attempt to open serial port.
                SerialPort? port = null;

                try
                {
                    port = new SerialPort(portName)
                    {
                        BaudRate = 38400
                    };
                    
                    port.Open();

                    if (!port.IsOpen)
                    {
                        port.Dispose();
                        continue;
                    }

                    _logger.LogDebug("Opened serial port {PortName}", portName);
                    
                    _evManager.RegisterDevice(port, protocol);

                    // Prevent disposal.
                    port = null;
                }
                catch (FileNotFoundException)
                {
                    _logger.LogError("No device detected on serial port {PortName}", portName);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to open serial port {PortName}", portName);
                }
                finally
                {
                    port?.Dispose();
                }
            }
        } while (await _periodicTimer.WaitForNextTickAsync(stoppingToken));
    }
}