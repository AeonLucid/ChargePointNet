using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using ChargePointNet.Packets;

namespace ChargePointNet.Core.Protocols.Max.Packets;

internal static class MaxPacketFrame
{
    public const int MinPacketLength = 13;
    
    private const int FrameMarkerStart = 0x02;
    private const int FrameMarkerEnd = 0x03;
    private const int FrameMarkerEOF = 0xFF;
    
    private static readonly byte[] AllowedPayloadBytes =
    [
        // \0, \r, \n, :
        0x00, 0x0a, 0x0d, 0x3a,
        // 0-9
        0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39,
        // A-Z
        0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E, 0x4F,
        0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59, 0x5A
    ];
    
    public static bool TryWritePacketPayload(Memory<byte> buffer, MaxPacket packet)
    {
        var writer = new HexSpanWriter(buffer.Span);
        
        writer.WriteU8(packet.Destination);
        writer.WriteU8(packet.Source);
        writer.WriteU8((byte)packet.Command);

        packet.Data?.Serialize(ref writer);

        return true;
    }

    public static bool TryWritePacketFrame(Memory<byte> buffer)
    {
        var span = buffer.Span;
        var payloadSpan = span.Slice(1, span.Length - 7);
        
        span[0] = FrameMarkerStart;
        span[^2] = FrameMarkerEnd;
        span[^1] = FrameMarkerEOF;
        
        CalculateChecksum(payloadSpan, span.Slice(span.Length - 6, 2));
        CalculateParity(payloadSpan, span.Slice(span.Length - 4, 2));

        return true;
    }

    /// <summary>
    ///     Find a packet frame that starts with 0x02 and ends with 0x03FF.
    /// </summary>
    public static bool TryFindPacketFrame(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> frame)
    {
        // TODO: Discard corrupted packets by adjusting buffer
        
        var reader = new SequenceReader<byte>(buffer);
        
        // Find 0x02.
        if (!reader.TryAdvanceTo(FrameMarkerStart, false))
        {
            frame = default;
            return false;
        }
        
        var startPos = reader.Position;

        // Find 0x03.
        if (!reader.TryAdvanceTo(FrameMarkerEnd))
        {
            frame = default;
            return false;
        }
        
        // Confirm 0xFF is after 0x03.
        if (!reader.TryRead(out var value) && value != FrameMarkerEOF)
        {
            frame = default;
            return false;
        }
        
        frame = buffer.Slice(startPos, reader.Position);
        buffer = buffer.Slice(frame.End);
        return true;
    }
    
    /// <summary>
    ///     Validate packet frame and extract payload.
    /// </summary>
    public static bool TryReadPacketFrame(ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> payload, out string? error)
    {
        payload = default;
        
        // Length.
        if (buffer.Length < MinPacketLength)
        {
            error = $"Invalid frame length: {buffer.Length}, expected at least {MinPacketLength} bytes";
            return false;
        }
        
        var sequenceReader = new SequenceReader<byte>(buffer);
        
        // Start of frame maker.
        if (!sequenceReader.TryPeek(0, out var firstByte) || firstByte != FrameMarkerStart)
        {
            error = $"Invalid start of frame marker 0x{firstByte:X2}";
            return false;
        }
        
        // End of frame marker.
        if (!sequenceReader.TryPeek(buffer.Length - 2, out var lastByte) || lastByte != FrameMarkerEnd)
        {
            error = $"Invalid end of frame marker 0x{lastByte:X2}";
            return false;
        }

        if (!sequenceReader.TryPeek(buffer.Length - 1, out var eofByte) || eofByte != FrameMarkerEOF)
        {
            error = $"Invalid end of frame EOF marker 0x{eofByte:X2}";
            return false;
        }

        // Payload.
        var frameSpanLen = (int)buffer.Length - 3;
        var frameSpan = frameSpanLen > 256 ? new byte[frameSpanLen] : stackalloc byte[frameSpanLen];
        
        sequenceReader.Advance(1);

        if (!sequenceReader.TryCopyTo(frameSpan))
        {
             error = "Failed to copy frame data";
             return false;
        }
        
        var payloadSpanLen = frameSpanLen - 4;
        var payloadSpan = frameSpan.Slice(0, payloadSpanLen);
        if (payloadSpan.ContainsAnyExcept(AllowedPayloadBytes))
        {
            error = "Invalid payload data";
            return false;
        }
        
        // Checksum.
        Span<byte> scratch = stackalloc byte[2];

        CalculateChecksum(payloadSpan, scratch);
        
        if (!scratch.SequenceEqual(frameSpan.Slice(payloadSpanLen, 2)))
        {
            error = "Invalid checksum";
            return false;
        }
        
        // Parity.
        CalculateParity(payloadSpan, scratch);

        if (!scratch.SequenceEqual(frameSpan.Slice(payloadSpanLen + 2, 2)))
        {
            error = "Invalid parity";
            return false;
        }

        payload = buffer.Slice(1, buffer.Length - 7);
        error = null;
        return true;
    }

    public static bool TryReadPacketPayload(ReadOnlySequence<byte> payload, [NotNullWhen(true)] out MaxPacket? packet)
    {
        var bufferLen = (int)payload.Length;
        var buffer = bufferLen > 256 ? new byte[bufferLen] : stackalloc byte[bufferLen];
        
        payload.CopyTo(buffer);
        
        var reader = new HexSpanReader(buffer);

        if (!reader.TryReadU8(out var destination))
        {
            packet = null;
            return false;
        }

        if (!reader.TryReadU8(out var source))
        {
            packet = null;
            return false;
        }

        if (!reader.TryReadU8(out var command))
        {
            packet = null;
            return false;
        }
        
        
        var commandType = (MaxCommand)command;
        
        IHexPacket? packetData = null;
        
        var packetDataType = MaxPacketDataMap.GetType(destination, source, commandType);
        if (packetDataType != null)
        {
            packetData = (IHexPacket)Activator.CreateInstance(packetDataType)!;

            if (!packetData.Deserialize(ref reader))
            {
                packet = null;
                return false;
            }
        }
        
        packet = new MaxPacket
        {
            Destination = destination,
            Source = source,
            Command = commandType,
            Data = packetData
        };

        return true;
    }

    public static void CalculateChecksum(ReadOnlySpan<byte> data, Span<byte> checksumBuffer)
    {
        if (checksumBuffer.Length < 2)
        {
            throw new ArgumentException("Checksum buffer must be at least 2 bytes long", nameof(checksumBuffer));
        }
        
        uint sum = 0;

        foreach (var b in data)
        {
            sum += b;
        }

        var checksum = (byte)sum;

        checksumBuffer[0] = ToHexUpper((byte)(checksum >> 4));
        checksumBuffer[1] = ToHexUpper((byte)(checksum & 0x0F));
    }

    public static void CalculateParity(ReadOnlySpan<byte> data, Span<byte> parityBuffer)
    {
        if (parityBuffer.Length < 2)
        {
            throw new ArgumentException("Parity buffer must be at least 2 bytes long", nameof(parityBuffer));
        }
        
        byte parity = 0;

        foreach (var b in data)
        {
            parity ^= b;
        }

        parityBuffer[0] = ToHexUpper((byte)(parity >> 4));
        parityBuffer[1] = ToHexUpper((byte)(parity & 0x0F));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte ToHexUpper(byte value) => (byte)(value < 10 ? '0' + value : 'A' + (value - 10));
}