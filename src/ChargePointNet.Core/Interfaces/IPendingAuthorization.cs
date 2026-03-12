namespace ChargePointNet.Core.Interfaces;

public interface IPendingAuthorization
{
    bool IsPending { get; }
    bool IsAuthorized { get; }
}