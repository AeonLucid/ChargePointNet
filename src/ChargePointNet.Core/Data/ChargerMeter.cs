namespace ChargePointNet.Core.Data;

public record ChargerMeter(ChargerMeterType? Type, string? Version, string? Model, string? Serial, double? MainsFrequency);