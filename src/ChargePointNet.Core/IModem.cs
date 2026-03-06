namespace ChargePointNet.Core;

/// <summary>
///     Represents a modem that also is a <see cref="IChargeBox"/> 
/// </summary>
public interface IModem : IDisposable
{
    bool Connected { get; }
    
    void Start();
    
    void Stop();
}