using ChargePointNet.Core.Interfaces;

namespace ChargePointNet.Services.Auth;

public interface IAuthService : IAuthRepository
{
    bool Authorize(Guid requestId, bool isAuthorized);

    IEnumerable<AuthRequest> GetPending();
}