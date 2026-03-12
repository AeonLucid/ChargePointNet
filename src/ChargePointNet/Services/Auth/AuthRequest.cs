using ChargePointNet.Core.Data;
using ChargePointNet.Core.Interfaces;

namespace ChargePointNet.Services.Auth;

public class AuthRequest : IPendingAuthorization
{
    public Guid Id { get; } = Guid.NewGuid();
    public required AuthRequestKey Key { get; init; }
    
    public bool IsPending { get; set; } = true;
    public bool IsAuthorized { get; set; }
    public bool IsExpired => DateTimeOffset.UtcNow > ExpiresAt;
    
    public required DateTimeOffset CreatedAt { get; init; }
    public required DateTimeOffset ExpiresAt { get; init; }
}