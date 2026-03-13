namespace ChargePointNet.Core.Protocols.Max;

internal static class MaxUtils
{
    private static readonly DateTime Epoch = new(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    
    /// <summary>
    ///     Retrieve time in seconds since 2000-1-1.
    /// </summary>
    /// <returns></returns>
    public static uint Timestamp()
    {
        return (uint)(DateTime.UtcNow - Epoch).TotalSeconds;
    }

    /// <summary>
    ///     Retrieve time in seconds since 2000-1-1 for a given date time.
    /// </summary>
    /// <param name="dateTime">The date time to convert to seconds since epoch.</param>
    /// <returns></returns>
    public static uint Timestamp(DateTimeOffset dateTime)
    {
        return (uint)(dateTime - Epoch).TotalSeconds;
    }
}