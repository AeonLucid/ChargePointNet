using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace ChargePointNet.Core.Protocols.Max.Packets.Serialization;

// Read data from a Span<byte>, advancing the position as it goes.
// Use BinaryPrimitives to read values from the Span.
internal ref struct SpanReader
{
    private ReadOnlySpan<byte> _data;
    
    public SpanReader(ReadOnlySpan<byte> data)
    {
        _data = data;
    }
    
    public int Remaining => _data.Length;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Advance(int count)
    {
        _data = _data.Slice(count);
    }
    
    public void Skip(int count)
    {
        Advance(count);
    }

    private bool TryReadPrimitive(scoped Span<byte> destination)
    {
        var hexLength = destination.Length * 2;
        
        if (_data.Length < hexLength)
        {
            return false;
        }
        
        if (Convert.FromHexString(_data.Slice(0, hexLength), destination, out _, out _) != OperationStatus.Done)
        {
            return false;
        }
        
        Advance(hexLength);
        return true;
    }
    
    public bool TryReadU8(out byte value)
    {
        Span<byte> output = stackalloc byte[1];
        
        if (!TryReadPrimitive(output))
        {
            value = 0;
            return false;
        }
        
        value = output[0];
        return true;
    }
    
    public bool TryReadU16(out ushort value)
    {
        Span<byte> output = stackalloc byte[2];
        
        if (!TryReadPrimitive(output))
        {
            value = 0;
            return false;
        }
        
        value = BinaryPrimitives.ReadUInt16BigEndian(output);
        return true;
    }
    
    public bool TryReadU32(out uint value)
    {
        Span<byte> output = stackalloc byte[4];
        
        if (!TryReadPrimitive(output))
        {
            value = 0;
            return false;
        }
        
        value = BinaryPrimitives.ReadUInt32LittleEndian(output);
        return true;
    }

    public bool TryReadString(Encoding encoding, int length, [NotNullWhen(true)] out string? value)
    {
        if (_data.Length < length)
        {
            value = null;
            return false;
        }

        value = encoding.GetString(_data.Slice(0, length));
        Advance(length);
        return true;
    }

    public bool TryReadBytes(int length, [NotNullWhen(true)] out byte[]? value)
    {
        if (_data.Length < length)
        {
            value = null;
            return false;
        }

        value = _data.Slice(0, length).ToArray();
        Advance(length);
        return true;
    }
}