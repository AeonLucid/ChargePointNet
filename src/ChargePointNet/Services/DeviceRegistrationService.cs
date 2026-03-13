using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using ChargePointNet.Config;
using ChargePointNet.Core;
using ChargePointNet.Core.Net;
using ChargePointNet.Core.Net.Devices;
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

    private async Task CheckDeviceAsync(IDevice device)
    {
        if (_evManager.IsRegistered(device.Identifier))
        {
            device.Dispose();
            return;
        }
        
        try
        {
            if (!await device.ConnectAsync())
            {
                _logger.LogError("Failed to connect with {Device}", device);
                
                device.Dispose();
                return;
            }

            _logger.LogDebug("Opened connection with {Device}", device);
            _evManager.RegisterDevice(device);
            
            return;
        }
        catch (SocketException)
        {
            _logger.LogError("Failed to connect with {Device}", device);
        }
        catch (FileNotFoundException)
        {
            _logger.LogError("No serial detected for {Device}", device);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to check {Device}", device);
        }

        device.Dispose();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DeviceDetectionService running");

        do
        {
            // Register new devices.
            var config = _devicesConfig.CurrentValue;

            foreach (var networkDevice in config.Network)
            {
                if (!networkDevice.Enabled) continue;
                
                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    Blocking = false
                };

                var remoteEndpoint = new IPEndPoint(IPAddress.Parse(networkDevice.IpAddress), networkDevice.Port);
                var device = new NetworkDevice(remoteEndpoint, socket, networkDevice.Protocol);
                
                await CheckDeviceAsync(device);
            }

            foreach (var serialDevice in config.Serial)
            {
                if (!serialDevice.Enabled) continue;
                
                var device = new SerialDevice(new SerialPort(serialDevice.Port)
                {
                    BaudRate = 38400
                }, serialDevice.Protocol);
                
                await CheckDeviceAsync(device);
            }
        } while (await _periodicTimer.WaitForNextTickAsync(stoppingToken));
    }
}