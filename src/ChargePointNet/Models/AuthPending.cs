namespace ChargePointNet.Models;

public class AuthPending
{
    /// <summary>
    ///     Unique identifier for the request.
    /// </summary>
    public required Guid RequestId { get; init; }
    
    /// <summary>
    ///     Serial number of the charger.
    /// </summary>
    public required string Serial { get; set; }
    
    /// <summary>
    ///     Card number of the NFC tag.
    /// </summary>
    public required string CardNumber { get; set; }

    /// <summary>
    ///     Time when the request was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }
    
    /// <summary>
    ///     Time when the request expires and can no longer be authorized.
    /// </summary>
    public DateTimeOffset ExpiresAt { get; set; }
}