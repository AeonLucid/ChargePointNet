using System.Text.Json.Serialization;
using ChargePointNet.Core.Data;

namespace ChargePointNet.Models;

public class MeterInfo
{
    public MeterInfo(ChargerMeter meter)
    {
        Type = meter.Type;
        Version = meter.Version;
        Model = meter.Model;
        Serial = meter.Serial;
        MainsFrequency = meter.MainsFrequency;
    }

    /// <summary>
    ///     Type of the meter.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ChargerMeterType Type { get; init; }
    
    /// <summary>
    ///     Version of the meter.
    /// </summary>
    public string Version { get; init; }
    
    /// <summary>
    ///     Model of the meter.
    /// </summary>
    public string Model { get; init; }
    
    /// <summary>
    ///     Serial number of the meter.
    /// </summary>
    public string Serial { get; init; }
    
    /// <summary>
    ///     Frequency of the mains supply in Hz.
    /// </summary>
    public double MainsFrequency { get; init; }
}