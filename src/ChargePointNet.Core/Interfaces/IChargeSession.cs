namespace ChargePointNet.Core.Interfaces;

public interface IChargeSession
{
    Guid Id { get; }
    
    bool IsCharging { get; set; }
    double? MeterValueStart { get; set; }
    double? MeterValueCurrent { get; set; }
    double? MeterValueEnd { get; set; }
    
    DateTimeOffset CreatedAt { get; }
    DateTimeOffset UpdatedAt { get; set; }
    DateTimeOffset? EndedAt { get; set; }
}