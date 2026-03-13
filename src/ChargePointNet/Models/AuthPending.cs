using ChargePointNet.Services.Auth;

namespace ChargePointNet.Models;

public class AuthPending
{
    public AuthPending(AuthRequest request)
    {
        RequestId = request.Id;
        Serial = request.Key.Serial;
        CardNumber = request.Key.CardNumber;
        CreatedAt = request.CreatedAt;
        ExpiresAt = request.ExpiresAt;
    }

    /// <summary>
    ///     Unique identifier for the request.
    /// </summary>
    public Guid RequestId { get; init; }

    /// <summary>
    ///     Serial number of the charger.
    /// </summary>
    public string Serial { get; init; }

    /// <summary>
    ///     Card number of the NFC tag.
    /// </summary>
    public string CardNumber { get; init; }

    /// <summary>
    ///     Time when the request was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    ///     Time when the request expires and can no longer be authorized.
    /// </summary>
    public DateTimeOffset ExpiresAt { get; init; }
}