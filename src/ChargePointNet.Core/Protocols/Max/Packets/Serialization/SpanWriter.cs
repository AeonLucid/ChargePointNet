using System.Runtime.CompilerServices;
using System.Text;

namespace ChargePointNet.Core.Protocols.Max.Packets.Serialization;

internal ref struct SpanWriter
{
    private readonly Span<byte> _data;

    public SpanWriter(Span<byte> data)
    {
        _data = data;
        Position = 0;
    }
    
    public int Position { get; private set; }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CheckBounds(int count)
    {
        if (Position + count > _data.Length)
        {
            throw new InvalidOperationException("Attempted to write past the end of the buffer.");
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Advance(int count)
    {
        Position += count;
    }
    
    /// <summary>
    ///     Write byte as hex string.
    /// </summary>
    public void WriteU8Hex(byte value)
    {
        CheckBounds(2);
        _data[Position] = ToHexUpper((byte)(value >> 4));
        _data[Position + 1] = ToHexUpper((byte)(value & 0x0F));
        Advance(2);
    }

    public void WriteU32Hex(int value)
    {
        CheckBounds(8);
        _data[Position] = ToHexUpper((byte)((value >> 28) & 0x0F));
        _data[Position + 1] = ToHexUpper((byte)((value >> 24) & 0x0F));
        _data[Position + 2] = ToHexUpper((byte)((value >> 20) & 0x0F));
        _data[Position + 3] = ToHexUpper((byte)((value >> 16) & 0x0F));
        _data[Position + 4] = ToHexUpper((byte)((value >> 12) & 0x0F));
        _data[Position + 5] = ToHexUpper((byte)((value >> 8) & 0x0F));
        _data[Position + 6] = ToHexUpper((byte)((value >> 4) & 0x0F));
        _data[Position + 7] = ToHexUpper((byte)(value & 0x0F));
        Advance(8);
    }
    
    public void WriteBytes(scoped ReadOnlySpan<byte> bytes)
    {
        CheckBounds(bytes.Length);
        
        if (bytes.Length == 0) return;
        
        bytes.CopyTo(_data.Slice(Position));
        Advance(bytes.Length);
    }
    
    public void WriteString(Encoding encoding, string value)
    {
        var byteCount = encoding.GetByteCount(value);
        if (byteCount > byte.MaxValue)
        {
            throw new InvalidOperationException("String is too long to write.");
        }
        
        Span<byte> bytes = stackalloc byte[byteCount];
        
        encoding.GetBytes(value, bytes);
        
        WriteBytes(bytes);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte ToHexUpper(byte value) => (byte)(value < 10 ? '0' + value : 'A' + (value - 10));
}