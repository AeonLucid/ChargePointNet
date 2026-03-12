namespace ChargePointNet.Core;

/// <summary>
///     Represents a modem that also is a <see cref="IChargeBox"/> 
/// </summary>
internal interface IModem : IDisposable
{
    IEnumerable<IChargeBox> Chargers { get; }
    
    bool Connected { get; }
    
    void Start();
    
    void Stop();
}