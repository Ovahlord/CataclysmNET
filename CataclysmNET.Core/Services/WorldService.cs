using CataclysmNET.Core.Database.Models;
using CataclysmNET.Core.Database.Tables.World;
using Microsoft.Extensions.Hosting;

namespace CataclysmNET.Core.Services;

public class WorldService : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using WorldDatabaseContext worldDb = new();
        if (await worldDb.Database.EnsureCreatedAsync(cancellationToken))
        {
            Console.WriteLine("World database has been created.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

