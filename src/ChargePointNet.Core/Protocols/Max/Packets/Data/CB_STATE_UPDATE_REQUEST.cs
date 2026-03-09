using ChargePointNet.Core.Protocols.Max.Data;
using ChargePointNet.Core.Protocols.Max.Packets.Serialization;

namespace ChargePointNet.Core.Protocols.Max.Packets.Data;

internal class CB_STATE_UPDATE_REQUEST : IMaxPacketData
{
    public ChargerBoxState State { get; set; }
    public byte IsCharging { get; set; }
    public byte LedColour { get; set; }
    public byte IsLocked { get; set; }
    public byte CableMaxCurrent { get; set; }
    public uint MeterValue { get; set; }
    public ushort ChassisTemperature { get; set; }
    public uint SessionId { get; set; }
    public ushort VoltagePhase1 { get; set; }
    public ushort VoltagePhase2 { get; set; }
    public ushort VoltagePhase3 { get; set; }
    public ushort CurrentPhase1 { get; set; }
    public ushort CurrentPhase2 { get; set; }
    public ushort CurrentPhase3 { get; set; }
    public ushort SocketTemperature { get; set; }
    public ushort PowerFactorPhase1 { get; set; }
    public ushort PowerFactorPhase2 { get; set; }
    public ushort PowerFactorPhase3 { get; set; }
    public ushort CurrentLimit { get; set; }
    public ushort MainsFrequency { get; set; }
    
    public int Size()
    {
        // TODO: 68 bytes on G2.
        return 132;
    }

    public void Serialize(ref SpanWriter writer)
    {
        throw new NotImplementedException();
    }

    public bool Deserialize(ref SpanReader reader)
    {
        if (!reader.TryReadU8(out var value))
        {
            return false;
        }
        
        State = (ChargerBoxState)value;
        
        reader.Skip(4); // Unknown.

        if (!reader.TryReadU8(out value))
        {
            return false;
        }
        
        IsCharging = value;

        if (!reader.TryReadU8(out value))
        {
            return false;
        }
        
        LedColour = value;

        if (!reader.TryReadU8(out value))
        {
            return false;
        }
        
        IsLocked = value;

        if (!reader.TryReadU8(out value))
        {
            return false;
        }

        CableMaxCurrent = value;
        
        reader.Skip(4);

        if (!reader.TryReadU32(out var u32))
        {
            return false;
        }
        
        MeterValue = u32 / 1000;
        
        reader.Skip(26);

        if (!reader.TryReadU16(out var u16))
        {
            return false;
        }
        
        ChassisTemperature = u16; // /10
        
        reader.Skip(2);
        
        if (!reader.TryReadU32(out u32))
        {
            return false;
        }
        
        SessionId = u32;
        
        reader.Skip(2);
        
        if (!reader.TryReadU16(out u16))
        {
            return false;
        }
        
        VoltagePhase1 = u16;
        
        if (!reader.TryReadU16(out u16))
        {
            return false;
        }
        
        VoltagePhase2 = u16;
        
        if (!reader.TryReadU16(out u16))
        {
            return false;
        }
        
        VoltagePhase3 = u16;
        
        if (!reader.TryReadU16(out u16))
        {
            return false;
        }
        
        CurrentPhase1 = u16;
        
        if (!reader.TryReadU16(out u16))
        {
            return false;
        }
        
        CurrentPhase2 = u16;
        
        if (!reader.TryReadU16(out u16))
        {
            return false;
        }
        
        CurrentPhase3 = u16;

        if (!reader.TryReadU16(out u16))
        {
            return false;
        }
        
        SocketTemperature = u16;

        if (!reader.TryReadU16(out u16))
        {
            return false;
        }
        
        PowerFactorPhase1 = u16;

        if (!reader.TryReadU16(out u16))
        {
            return false;
        }
        
        PowerFactorPhase2 = u16;

        if (!reader.TryReadU16(out u16))
        {
            return false;
        }
        
        PowerFactorPhase3 = u16;
        
        reader.Skip(16);
        
        if (!reader.TryReadU16(out u16))
        {
            return false;
        }
        
        CurrentLimit = u16;
        
        if (!reader.TryReadU16(out u16))
        {
            return false;
        }
        
        MainsFrequency = u16;
        return true;
    }
}