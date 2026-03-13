using ChargePointNet.Core.Interfaces;

namespace ChargePointNet.Services.Sessions;

public interface ISessionService : ISessionRepository
{
    IEnumerable<ChargeSession> GetAll();
}