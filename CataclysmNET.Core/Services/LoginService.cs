using CataclysmNET.Core.Database.Models;
using CataclysmNET.Core.Database.Tables.Login;
using Microsoft.Extensions.Hosting;

namespace CataclysmNET.Core.Services;

public class LoginService : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using LoginDatabaseContext loginDb = new();
        if (await loginDb.Database.EnsureCreatedAsync(cancellationToken))
        {
            loginDb.LoginInstances.Add(new LoginInstance());
            await loginDb.SaveChangesAsync(cancellationToken);
            Console.WriteLine("Login database has been created and set up.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
