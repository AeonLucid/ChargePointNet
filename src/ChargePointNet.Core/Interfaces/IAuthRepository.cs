using ChargePointNet.Core.Data;

namespace ChargePointNet.Core.Interfaces;

public interface IAuthRepository
{
    /// <summary>
    ///     Get or create a pending auth request.
    /// </summary>
    IPendingAuthorization GetOrCreate(AuthorizationContext key, TimeSpan timeout);
    
    /// <summary>
    ///     Remove an auth request from the data store after consumption.
    /// </summary>
    void Remove(AuthorizationContext key);
}