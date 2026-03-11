namespace ChargePointNet.Packets.Max;

public partial class CB_STATE_UPDATE_REQUEST : IHexPacket
{
    public byte State { get; set; }
    [PacketField(FixedSize = 4, Raw = true)]
    private byte[]? Unknown_2 { get; set; }
    public byte IsCharging { get; set; }
    public byte LedColour { get; set; }
    public byte IsLocked { get; set; }
    public byte CableMaxCurrent { get; set; }
    [PacketField(FixedSize = 4, Raw = true)]
    private byte[]? Unknown_14 { get; set; }
    public uint MeterValue { get; set; }
    [PacketField(FixedSize = 26, Raw = true)]
    private byte[]? Unknown_26 { get; set; }
    public ushort ChassisTemperature { get; set; }
    [PacketField(FixedSize = 2, Raw = true)]
    private byte[]? Unknown_56 { get; set; }
    public uint SessionId { get; set; }
    [PacketField(FixedSize = 2, Raw = true)]
    private byte[]? Unknown_66 { get; set; }
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
    [PacketField(FixedSize = 16, Raw = true)]
    private byte[]? Unknown_108 { get; set; }
    public ushort CurrentLimit { get; set; }
    public ushort MainsFrequency { get; set; }
}