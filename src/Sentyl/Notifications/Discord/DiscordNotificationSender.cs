using Microsoft.Extensions.Options;

using NetCord.Rest;

using Sentyl.ChangeDetection;
using Sentyl.Untis;

namespace Sentyl.Notifications.Discord;

public sealed class DiscordNotificationSender(
    IOptions<DiscordOptions> options,
    MasterDataProvider masterDataProvider) : INotificationSender
{
    public async Task SendAsync(LessonChangeGroup group, CancellationToken cancellationToken)
    {
        MasterData masterData = await masterDataProvider.GetAsync(cancellationToken);

        using var webhookClient = new WebhookClient(ulong.Parse(options.Value.WebhookId), options.Value.WebhookToken);

        await webhookClient.ExecuteAsync(new WebhookMessageProperties
        {
            Content = BuildContent(group, masterData)
        }, cancellationToken: cancellationToken);
    }

    private static string BuildContent(LessonChangeGroup group, MasterData masterData)
    {
        DiscordMessageFormatter formatter = new(masterData);
        string message = formatter.Format(group);

        return group.Severity == NotificationSeverity.Ping
            ? $"@everyone\n{message}"
            : message;
    }
}
