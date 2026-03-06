using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using ChargePointNet.Config;
using ChargePointNet.Core;
using ChargePointNet.Core.Net;
using ChargePointNet.Core.Net.Devices;
using ChargePointNet.Core.Protocols;
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

    private async Task CheckDeviceAsync(IDevice device, EVProtocol protocol)
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
            _evManager.RegisterDevice(device, protocol);
            
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
            // Clean up.
            _evManager.Cleanup();
            
            // Register new devices.
            var config = _devicesConfig.CurrentValue;

            foreach (var endpoint in config.Endpoints)
            {
                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    Blocking = false
                };

                var remoteEndpoint = new IPEndPoint(IPAddress.Parse(endpoint.IpAddress), endpoint.Port);
                var device = new NetworkDevice(remoteEndpoint, socket);
                
                await CheckDeviceAsync(device, endpoint.Protocol);
                
                // var portName = endpoint.ToString();
                //
                // if (_evManager.IsRegistered(portName))
                // {
                //     continue;
                // }
                //
                // // Attempt to open socket.
                // Socket? socket = null;
                //
                // try
                // {
                //     socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                //     {
                //         Blocking = false
                //     };
                //
                //     await socket.ConnectAsync(new IPEndPoint(IPAddress.Parse(endpoint.IpAddress), endpoint.Port), stoppingToken);
                //
                //     _logger.LogDebug("Connected to remote endpoint {Endpoint}", portName);
                //
                //     _evManager.RegisterDevice(new NetworkDevice(portName, socket), EVProtocol.Max);
                //
                //     socket = null;
                // }
                // catch (SocketException)
                // {
                //     _logger.LogError("No device detected at endpoint {Endpoint}", endpoint);
                // }
                // catch (Exception e)
                // {
                //     _logger.LogError(e, "Failed to connect to device at {Endpoint}", endpoint);
                // }
                // finally
                // {
                //     socket?.Dispose();
                // }
            }

            foreach (var (portName, protocol) in config.Ports)
            {
                var device = new SerialDevice(new SerialPort(portName)
                {
                    BaudRate = 38400
                });
                
                await CheckDeviceAsync(device, protocol);
            }
        } while (await _periodicTimer.WaitForNextTickAsync(stoppingToken));
    }
}