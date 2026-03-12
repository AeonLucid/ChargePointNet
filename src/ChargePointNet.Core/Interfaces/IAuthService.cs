using ChargePointNet.Core.Data;

namespace ChargePointNet.Core.Interfaces;

public interface IAuthService
{
    /// <summary>
    ///     Get or create a pending auth request.
    /// </summary>
    IPendingAuthorization GetOrCreate(AuthRequestKey key, TimeSpan timeout);
    
    /// <summary>
    ///     Remove an auth request from the data store after consumption.
    /// </summary>
    void Remove(AuthRequestKey key);
}