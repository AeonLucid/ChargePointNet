namespace ChargePointNet.Models;

/// <summary>
///     List of charging sessions.
/// </summary>
public class SessionList
{
    public required IEnumerable<Session> Results { get; set; }
}