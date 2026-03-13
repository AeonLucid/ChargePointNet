namespace ChargePointNet.Models.Requests;

public class AutostartUpdateRequest
{
    /// <summary>
    ///     Whether autostart should be enabled or disabled.
    /// </summary>
    public required bool Enabled { get; set; }
}