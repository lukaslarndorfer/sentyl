using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;
using Sentyl;
using Sentyl.Discord;
using Sentyl.Discord.Notifications;
using Sentyl.Notifications;
using Sentyl.Persistence;
using Sentyl.Security;
using Sentyl.Timetable;
using Sentyl.Untis;
using Sentyl.Untis.Client;

var builder = Host.CreateApplicationBuilder(args);

string keysPath = builder.Configuration[$"{StorageOptions.SectionName}:{nameof(StorageOptions.KeysPath)}"]
                  ?? throw new InvalidOperationException("Storage:KeysPath is not configured");

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

// temporary; removed once polling runs per guild
builder.Services.AddSingleton(serviceProvider =>
{
    UntisOptions options = serviceProvider.GetRequiredService<IOptions<UntisOptions>>().Value;

    return UntisCredentials.Create(options.Host, options.School, options.User, options.Password);
});

builder.Services.AddHttpClient(UntisClientFactory.HttpClientName)
       .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
       {
           UseCookies = false,
           // the session cookie is set manually, so a redirect must never forward it to another host
           AllowAutoRedirect = false
       })
       .AddStandardResilienceHandler();

builder.Services.ConfigurePersistence(builder.Environment.IsDevelopment());

builder.Services.AddDiscordGateway()
       .AddApplicationCommands();

builder.Services
       .AddDataProtection()
       .SetApplicationName("Sentyl")
       .PersistKeysToFileSystem(new DirectoryInfo(keysPath));

builder.Services.AddTransient<MasterDataLoader>();
builder.Services.AddTransient<TimetablePoller>();

builder.Services.AddSingleton<INotificationSender, DiscordNotificationSender>();
builder.Services.AddSingleton<ITimetableStateStore, DatabaseTimetableStateStore>();
builder.Services.AddSingleton<MasterDataProvider>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IUntisClientFactory, UntisClientFactory>();
builder.Services.AddSingleton<IUntisCredentialsValidator, UntisCredentialsValidator>();
builder.Services.AddSingleton<ISecretProtector, DataProtectionSecretProtector>();
builder.Services.AddSingleton(sp => sp.GetRequiredService<IUntisClientFactory>()
                                      .Create(sp.GetRequiredService<UntisCredentials>()));
builder.Services.AddSingleton<ITimetableProvider>(sp => sp.GetRequiredService<UntisClient>());

builder.Services.AddHostedService<Worker>();

var host = builder.Build();

host.AddModules(typeof(Program).Assembly);

await host.MigrateDatabaseAsync();
await host.RunAsync();
