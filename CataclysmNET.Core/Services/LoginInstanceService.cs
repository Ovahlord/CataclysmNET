using CataclysmNET.Core.Utils;
using Microsoft.Extensions.Hosting;

namespace CataclysmNET.Core.Services;

public class LoginInstanceService : IHostedService
{
    private readonly InstanceHostArguments _arguments;

    public LoginInstanceService(InstanceHostArguments arguments)
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
