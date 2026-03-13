using ChargePointNet.Core.Interfaces;

namespace ChargePointNet.Services.Sessions;

public interface ISessionService : ISessionRepository
{
    ChargeSession? Find(Guid id);
    
    IEnumerable<ChargeSession> GetAll();
}