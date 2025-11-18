using CataclysmNET.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

HostApplicationBuilder builder = Host.CreateApplicationBuilder();

builder.Services.AddHostedService<LoginService>();
builder.Services.AddHostedService<RealmService>();
builder.Services.AddHostedService<WorldService>();

using IHost host = builder.Build();

await host.RunAsync();
