using ChargePointNet.Core.Data;

namespace ChargePointNet.Core.Interfaces;

public interface ISessionRepository
{
    IChargeSession CreateSession(AuthorizationContext key);
    
    // TODO: Do we want to clear sessions when a charger reconnects? It should have lost its memory if it rebooted.
    // void Clear(string serial);
}