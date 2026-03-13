namespace ChargePointNet.Config;

public class AuthConfig
{
    public const string Section = "Auth";
    
    public bool Automatic { get; set; }
    public HashSet<string> AllowedList { get; set; } = [];
}