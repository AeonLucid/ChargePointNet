using System.Buffers;
using System.Collections.Concurrent;
using System.IO.Pipelines;
using ChargePointNet.Core.Net;
using Serilog;

namespace ChargePointNet.Core.Protocols.Max;

public class MaxModemBus : IDisposable
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
        catch (Exception e)
        {
            Logger.Error(e, "Error in read/write loop from device {PortName}", _device.Identifier);
        }
        finally
        {
            Logger.Verbose("Bus read/write loop stopped for device {PortName}", _device.Identifier);
            
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
            await _device.WriteAsync(packet.GetData());
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
                    while (TryReadPacket(ref buffer, out var packet))
                    {
                        Logger.Verbose("Received packet from {Identifier}: {Packet}", _device.Identifier, Convert.ToHexStringLower(packet.ToArray()));
                        
                        // TODO: Verify packet checksum / parity
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

    /// <summary>
    ///     Valid packet frame starts with 0x02 and ends with 0x03FF.
    /// </summary>
    private static bool TryReadPacket(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> packet)
    {
        // TODO: Discard corrupted packets
        
        var reader = new SequenceReader<byte>(buffer);
        
        // Find 0x02.
        if (!reader.TryAdvanceTo(0x02, false))
        {
            packet = default;
            return false;
        }
        
        var startPos = reader.Position;

        // Find 0x03.
        if (!reader.TryAdvanceTo(0x03))
        {
            packet = default;
            return false;
        }
        
        // Confirm 0xFF is after 0x03.
        if (!reader.TryRead(out var value) && value != 0xFF)
        {
            packet = default;
            return false;
        }
        
        packet = buffer.Slice(startPos, reader.Position);
        buffer = buffer.Slice(packet.End);
        return true;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        
        _disposed = true;
        _device.Dispose();
    }
}