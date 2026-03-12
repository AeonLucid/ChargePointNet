using System.Collections.Concurrent;
using ChargePointNet.Core.Data;
using ChargePointNet.Core.Interfaces;

namespace ChargePointNet.Services.Auth;

public class InMemoryAuthService : IAuthService
{
    private readonly ConcurrentDictionary<AuthRequestKey, AuthRequest> _requests;
    
    public InMemoryAuthService()
    {
        _requests = [];
    }

    public IPendingAuthorization GetOrCreate(AuthRequestKey key, TimeSpan timeout)
    {
        var time = DateTimeOffset.UtcNow;

        return _requests.AddOrUpdate(key, _ => new AuthRequest
        {
            Key = key,
            CreatedAt = time,
            ExpiresAt = time.Add(timeout),
        }, (_, request) =>
        {
            // Request has expired, create a new one.
            if (request.IsExpired)
            {
                return new AuthRequest
                {
                    Key = key,
                    CreatedAt = time,
                    ExpiresAt = time.Add(timeout)
                };
            }

            // Request is still valid, return it.
            return request;
        });
    }

    public void Remove(AuthRequestKey key)
    {
        _requests.Remove(key, out _);
    }

    public bool Authorize(Guid requestId, bool isAuthorized)
    {
        var request = GetPending().FirstOrDefault(x => x.Id == requestId);
        if (request == null)
        {
            return false;
        }

        request.IsAuthorized = isAuthorized;
        request.IsPending = false;
        return true;
    }

    public IEnumerable<AuthRequest> GetPending()
    {
        return _requests.Values.Where(x => !x.IsExpired && x.IsPending);
    }
}