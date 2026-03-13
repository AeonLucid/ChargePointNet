using System.Collections.Concurrent;
using ChargePointNet.Core.Data;
using ChargePointNet.Core.Interfaces;

namespace ChargePointNet.Services.Sessions;

// TODO: Clean up sessions periodically. X days?
public class SessionServiceInMemory : ISessionService
{
    private readonly ConcurrentDictionary<string, List<ChargeSession>> _sessions;
    
    public SessionServiceInMemory()
    {
        _sessions = [];
    }

    public IChargeSession CreateSession(AuthorizationContext key)
    {
        var store = _sessions.GetOrAdd(key.Serial, _ => []);
        var session = new ChargeSession
        {
            Key = key
        };
        
        store.Add(session);

        return session;
    }

    public ChargeSession? Find(Guid id)
    {
        return _sessions.Values.SelectMany(x => x).FirstOrDefault(x => x.Id == id);
    }

    public IEnumerable<ChargeSession> GetAll()
    {
        return _sessions.Values.SelectMany(x => x);
    }
}