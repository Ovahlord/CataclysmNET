using CataclysmNET.Core.Services;
using CataclysmNET.Core.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

if (!InstanceHostArguments.TryParse(args, out InstanceHostArguments? arguments))
{
    Console.WriteLine("Failed to parse command line arguments for InstanceHost process. Process is shutting down");
    return;
}

HostApplicationBuilder builder = Host.CreateApplicationBuilder();
builder.Services.AddSingleton(arguments);

switch (arguments.InstanceType)
{
    case "Login":
        builder.Services.AddHostedService<LoginInstanceService>();
        break;
    case "Realm":
        builder.Services.AddHostedService<RealmInstanceService>();
        break;
    case "World":
        builder.Services.AddHostedService<WorldInstanceService>();
        break;
    default:
    {
        Console.WriteLine("Unknown InstanceType argument. InstanceHost process is shutting down.");
        return;
    };
}

IHost app = builder.Build();
await app.RunAsync();
