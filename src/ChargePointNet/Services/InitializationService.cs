using ChargePointNet.Core;

namespace ChargePointNet.Services;

public class InitializationService : IHostedService
{
    private readonly EVManager _evManager;

    public InitializationService(EVManager evManager)
    {
        _evManager = evManager;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _evManager.Start();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _evManager.Stop();
        return Task.CompletedTask;
    }
}