using ChargePointNet.Core.Data;
using ChargePointNet.Core.Interfaces;

namespace ChargePointNet.Services.Sessions;

public class ChargeSession : IChargeSession
{
    public required AuthorizationContext Key { get; init; }
    
    public Guid Id { get; } = Guid.NewGuid();
    public bool IsCharging { get; set; }
    public double? MeterValueStart { get; set; }
    public double? MeterValueCurrent { get; set; }
    public double? MeterValueEnd { get; set; }
    public DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? EndedAt { get; set; }
}