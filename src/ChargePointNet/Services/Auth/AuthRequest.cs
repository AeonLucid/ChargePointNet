using ChargePointNet.Core.Data;
using ChargePointNet.Core.Interfaces;

namespace ChargePointNet.Services.Auth;

public class AuthRequest : IPendingAuthorization
{
    public required AuthorizationContext Key { get; init; }
    
    public Guid Id { get; } = Guid.NewGuid();
    
    public bool IsPending { get; set; } = true;
    public bool IsAuthorized { get; set; }
    public bool IsExpired => DateTimeOffset.UtcNow > ExpiresAt;
    
    public required DateTimeOffset CreatedAt { get; init; }
    public required DateTimeOffset ExpiresAt { get; init; }
}