using CataclysmNET.Core.Utils;
using Microsoft.Extensions.Hosting;

namespace CataclysmNET.Core.Services;

public class WorldInstanceService : IHostedService
{
    private readonly InstanceHostArguments _arguments;

    public WorldInstanceService(InstanceHostArguments arguments)
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
