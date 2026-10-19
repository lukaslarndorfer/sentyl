using System.Net;

using Microsoft.Extensions.Options;

using UntisSentinel;
using UntisSentinel.Notifications;
using UntisSentinel.Timetable;
using UntisSentinel.Untis;

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

builder.Services.AddHttpClient<UntisClient>()
    .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
    {
        UseCookies = true,
        CookieContainer = cookieContainer
    })
    .AddStandardResilienceHandler();

builder.Services.AddTransient<MasterDataLoader>();
builder.Services.AddSingleton<ITimetableProvider, UntisClient>();

builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton<INotificationSender, LoggingNotificationSender>();
builder.Services.AddSingleton<ITimetableStateStore, InMemoryTimetableStateStore>();
builder.Services.AddTransient<TimetablePoller>();

var host = builder.Build();
host.Run();
