namespace ChargePointNet.Models;

public class ChargerList
{
    /// <summary>
    ///     List containing connected and initialized chargers.
    /// </summary>
    public required IEnumerable<Charger> Results { get; set; }
}