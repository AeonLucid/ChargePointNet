namespace ChargePointNet.Packets.Max;

public partial class SET_CURRENT_LIMIT_REQUEST : IHexPacket
{
    public byte Unknown { get; set; }
    public ushort MinimumCurrent { get; set; }
    public ushort CurrentLimitPhase1 { get; set; }
    public ushort CurrentLimitPhase2 { get; set; }
    public ushort CurrentLimitPhase3 { get; set; }
}