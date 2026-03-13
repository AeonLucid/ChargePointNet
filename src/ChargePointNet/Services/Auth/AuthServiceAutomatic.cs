using ChargePointNet.Config;
using ChargePointNet.Core.Data;
using ChargePointNet.Core.Interfaces;
using Microsoft.Extensions.Options;

namespace ChargePointNet.Services.Auth;

public class AuthServiceAutomatic : IAuthService
{
    private readonly ILogger<AuthServiceAutomatic> _logger;
    private readonly IOptionsMonitor<AuthConfig> _options;

    public AuthServiceAutomatic(ILogger<AuthServiceAutomatic> logger, IOptionsMonitor<AuthConfig> options)
    {
        _logger = logger;
        _options = options;
    }

    public IPendingAuthorization GetOrCreate(AuthorizationContext key, TimeSpan timeout)
    {
        var authorized = _options.CurrentValue.AllowedList.Contains(key.CardNumber);

        _logger.LogInformation("Automatic authorization for serial {Serial}, card number {CardNumber}: {Authorized}", key.Serial, key.CardNumber, authorized);
        
        return new AuthRequest
        {
            Key = key,
            IsPending = false,
            IsAuthorized = authorized,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.Add(timeout),
        };
    }

    public void Remove(AuthorizationContext key)
    {
    }

    public bool Authorize(Guid requestId, bool isAuthorized)
    {
        return false;
    }

    public IEnumerable<AuthRequest> GetPending()
    {
        return [];
    }
}