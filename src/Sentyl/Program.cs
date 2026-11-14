using System.Net;

using Microsoft.Extensions.Options;

using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;

using Sentyl;
using Sentyl.Discord;
using Sentyl.Discord.Notifications;
using Sentyl.Notifications;
using Sentyl.Persistence;
using Sentyl.Timetable;
using Sentyl.Untis;

var builder = Host.CreateApplicationBuilder(args);
var cookieContainer = new CookieContainer();

builder.Services
    .AddOptions<UntisOptions>()
    .BindConfiguration(UntisOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services
    .AddOptions<PollingOptions>()
    .BindConfiguration(PollingOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services
    .AddOptions<DiscordOptions>()
    .BindConfiguration(DiscordOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services
    .AddOptions<StorageOptions>()
    .BindConfiguration(StorageOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddHttpClient<UntisClient>()
    .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
    {
        UseCookies = true,
        CookieContainer = cookieContainer
    })
    .AddStandardResilienceHandler();

builder.Services.ConfigurePersistence(builder.Environment.IsDevelopment());

builder.Services.AddDiscordGateway()
       .AddApplicationCommands();

builder.Services.AddTransient<MasterDataLoader>();
builder.Services.AddTransient<TimetablePoller>();

builder.Services.AddSingleton<ITimetableProvider, UntisClient>();
builder.Services.AddSingleton<INotificationSender, DiscordNotificationSender>();
builder.Services.AddSingleton<ITimetableStateStore, DatabaseTimetableStateStore>();
builder.Services.AddSingleton<MasterDataProvider>();
builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddHostedService<Worker>();

var host = builder.Build();

host.AddModules(typeof(Program).Assembly);

await host.MigrateDatabaseAsync();
await host.RunAsync();
