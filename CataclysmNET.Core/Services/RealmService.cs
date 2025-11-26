using CataclysmNET.Core.Database.Models;
using CataclysmNET.Core.Database.Tables.Realm;
using Microsoft.Extensions.Hosting;

namespace CataclysmNET.Core.Services;

public class RealmService : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using RealmDatabaseContext realmDb = new();
        if (await realmDb.Database.EnsureCreatedAsync(cancellationToken))
        {
            realmDb.RealmInstances.Add(new RealmInstance());
            await realmDb.SaveChangesAsync(cancellationToken);
            Console.WriteLine("Realm database has been created and set up.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
