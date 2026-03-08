using System.Buffers;
using System.Collections.Concurrent;
using System.IO.Pipelines;
using System.Net.Sockets;
using ChargePointNet.Core.Net;
using ChargePointNet.Core.Protocols.Max.Packets;
using Serilog;

namespace ChargePointNet.Core.Protocols.Max;

internal class MaxModemBus : IDisposable
{
    private static readonly ILogger Logger = Log.ForContext<MaxModemBus>();
    
    private const int MaximumBufferSize = 256;
    
    private readonly IDevice _device;
    private readonly Pipe _receivePipe;
    private readonly ConcurrentQueue<MaxPacket> _outBuffer;

    private bool _stopped;
    private bool _disposed;
    private Task? _connectionTask;

    public MaxModemBus(IDevice device)
    {
        _device = device;
        _receivePipe = new Pipe(new PipeOptions());
        _outBuffer = [];
    }

    public bool Connected => _device.Connected && _connectionTask != null && !_stopped && !_disposed;
    public Func<MaxPacket, Task>? OnPacketReceived { get; set; }
    
    public void Send(MaxPacket packet)
    {
        _outBuffer.Enqueue(packet);
    }

    public void Start()
    {
        if (_connectionTask != null)
        {
            throw new InvalidOperationException("Bus is already running");
        }

        var readWriteTask = Task.Run(ReadWriteAsync);
        var processTask = Task.Run(ProcessAsync);
        
        _connectionTask = Task.WhenAll(readWriteTask, processTask);
    }

    public void Stop()
    {
        if (_stopped)
        {
            return;
        }

        _stopped = true;
        _outBuffer.Clear();
        
        try
        {
            _device.Close();
        }
        catch (Exception)
        {
            // ignored
        }
    }

    private async Task ReadWriteAsync()
    {
        try
        {
            while (true)
            {
                if (!_device.Connected)
                {
                    break;
                }
                
                // Read.
                if (!await ReadAsync())
                {
                    break;
                }
            
                // Write.
                if (!_outBuffer.IsEmpty)
                {
                    await WriteAsync();
                }
            }
        }
        catch (SocketException e) when (e.SocketErrorCode == SocketError.ConnectionReset)
        {
            Logger.Warning("{Device}: Connection closed by remote", _device);
        }
        catch (Exception e)
        {
            Logger.Error(e, "{Device}: Error in read/write loop", _device);
        }
        finally
        {
            Logger.Verbose("{Device}: Bus read/write loop stopped", _device);
            
            await _receivePipe.Writer.CompleteAsync();
            
            Stop();
        }
    }

    private async Task<bool> ReadAsync()
    {
        var memory = _receivePipe.Writer.GetMemory(MaximumBufferSize);
        var bytesRead = await _device.ReadAsync(memory);
        if (bytesRead != 0)
        {
            _receivePipe.Writer.Advance(bytesRead);
        
            var result = await _receivePipe.Writer.FlushAsync();
            if (result.IsCompleted)
            {
                return false;
            }
        }
        
        return true;
    }
    
    private async Task WriteAsync()
    {
        while (_outBuffer.TryDequeue(out var packet))
        {
            var packetLen = MaxPacketFrame.MinPacketLength + (packet.Data?.Size() ?? 0);
            
            using var buffer = MemoryPool<byte>.Shared.Rent(packetLen);

            var packetBuffer = buffer.Memory.Slice(0, packetLen);
            var packetPayload = buffer.Memory.Slice(1, packetLen - 7);

            if (!MaxPacketFrame.TryWritePacketPayload(packetPayload, packet))
            {
                Logger.Warning("{Device}: Failed to write packet command 0x{Packet:X2}", _device, packet.Command);
                continue;
            }

            if (!MaxPacketFrame.TryWritePacketFrame(packetBuffer))
            {
                Logger.Warning("{Device}: Failed to write packet frame command 0x{Packet:X2}", _device, packet.Command);
                continue;
            }

            if (!MaxPacketFrame.TryReadPacketFrame(new ReadOnlySequence<byte>(packetBuffer), out _, out var error))
            {
                Logger.Warning("{Device}: Failed to validate packet frame command 0x{Packet:X2}: {Error}", _device, packet.Command, error);
                continue;
            }
            
            Logger.Debug("{Device}: Sending to 0x{Address:X2} {Command} ({@Data})", _device, packet.Destination, packet.Command, packet.Data);
            
            await _device.WriteAsync(packetBuffer);
        }
    }

    private async Task ProcessAsync()
    {
        var reader = _receivePipe.Reader;

        try
        {
            while (true)
            {
                var result = await reader.ReadAsync();
                if (result.IsCanceled)
                {
                    break;
                }
                
                var buffer = result.Buffer;

                try
                {
                    while (MaxPacketFrame.TryFindPacketFrame(ref buffer, out var frame))
                    {
                        Logger.Verbose("{Device}: Received packet {Packet}", _device, Convert.ToHexStringLower(frame.ToArray()));

                        if (!MaxPacketFrame.TryReadPacketFrame(frame, out var payload, out var error))
                        {
                            Logger.Warning("{Device}: Failed to read packet payload: {Error}", _device, error);
                            continue;
                        }

                        if (!MaxPacketFrame.TryReadPacketPayload(payload, out var packet))
                        {
                             Logger.Warning("{Device}: Failed to read packet: {Error}", _device, error);
                             continue;
                        }
                        
                        Logger.Debug("{Device}: Received from 0x{Address:X2} {Command} {@Data}", _device, packet.Source, packet.Command, packet.Data);

                        if (OnPacketReceived != null)
                        {
                            try
                            {
                                await OnPacketReceived(packet);
                            }
                            catch (Exception e)
                            {
                                Logger.Error(e, "{Device}: Error processing packet: {@Packet}", _device, packet);
                            }
                        }
                    }

                    if (result.IsCompleted)
                    {
                        break;
                    }
                }
                finally
                {
                    reader.AdvanceTo(buffer.Start, buffer.End);
                }
            }
        }
        catch (Exception e)
        {
            Logger.Error(e, "Error processing data from {PortName}", _device.Identifier);
        }
        finally
        {
            Logger.Verbose("Bus process loop stopped from {PortName}", _device.Identifier);
            
            await reader.CompleteAsync();
            
            Stop();
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        
        _disposed = true;
        _outBuffer.Clear();
        _device.Dispose();
    }
}