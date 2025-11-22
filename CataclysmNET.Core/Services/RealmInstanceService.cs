using CataclysmNET.Core.Utils;
using Microsoft.Extensions.Hosting;

namespace CataclysmNET.Core.Services;

public class RealmInstanceService : IHostedService
{
    private readonly InstanceHostArguments _arguments;

    public RealmInstanceService(InstanceHostArguments arguments)
    {
        _arguments = arguments;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
