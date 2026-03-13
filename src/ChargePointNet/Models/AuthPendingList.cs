namespace ChargePointNet.Models;

/// <summary>
///     List of pending authorization requests.
/// </summary>
public class AuthPendingList
{
    public required IEnumerable<AuthPending> Results { get; set; }
}