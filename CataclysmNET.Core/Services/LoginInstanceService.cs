using CataclysmNET.Core.Database.Models;
using CataclysmNET.Core.Database.Tables.Login;
using CataclysmNET.Core.Networking;
using CataclysmNET.Core.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Net;

namespace CataclysmNET.Core.Services;

public class LoginInstanceService : IHostedService
{
    private readonly InstanceHostArguments _arguments;
    private ConnectionListener<LoginSession>? _connectionListener;
    private Task? _listenerTask;

    public LoginInstanceService(InstanceHostArguments arguments)
    {
        _arguments = arguments;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return;

        await using LoginDatabaseContext context = new();
        LoginInstance? instance = await context.LoginInstances
            .FirstOrDefaultAsync(i => i.Id == _arguments.Parameter1, cancellationToken)
            .ConfigureAwait(false);

        if (instance == null)
            throw new Exception("The provided Login Instance Id does not have an entry in the database. Service cannot be started.");

        IPEndPoint endPoint = IPEndPoint.Parse(instance.EndPointAddress);
        _connectionListener = new ConnectionListener<LoginSession>(endPoint);
        _listenerTask = _connectionListener.StartAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_connectionListener != null)
            await _connectionListener.StopAsync(cancellationToken).ConfigureAwait(false);

        if (_listenerTask != null)
            await _listenerTask.WaitAsync(cancellationToken).ConfigureAwait(false);
    }
}
