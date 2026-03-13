namespace ChargePointNet.Models;

/// <summary>
///     List containing initialized and connected chargers.
/// </summary>
public class ChargerList
{
    public required IEnumerable<Charger> Results { get; set; }
}