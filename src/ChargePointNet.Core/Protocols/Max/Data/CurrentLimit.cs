namespace ChargePointNet.Core.Protocols.Max.Data;

public record CurrentLimit(ushort MinimumCurrent, ushort Phase1, ushort Phase2, ushort Phase3);