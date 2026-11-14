using Microsoft.Extensions.Options;

using NetCord;

using NetCord.Rest;

using Sentyl.ChangeDetection;
using Sentyl.Notifications;
using Sentyl.Untis;

namespace Sentyl.Discord.Notifications;

public sealed class DiscordNotificationSender(
    IOptions<DiscordOptions> options,
    MasterDataProvider masterDataProvider) : INotificationSender
{
    public async Task SendAsync(LessonChangeGroup group, CancellationToken cancellationToken)
    {
        bool isPing = group.Severity == NotificationSeverity.Ping;
        MasterData masterData = await masterDataProvider.GetAsync(cancellationToken);

        using var client = new RestClient(new BotToken(options.Value.Token));

        await client.SendMessageAsync(
        options.Value.ChannelId,
        new MessageProperties
        {
            Content = BuildContent(group, masterData, isPing),
            AllowedMentions = BuildAllowedMentions(isPing)
        }, cancellationToken: cancellationToken);
    }

    private static string BuildContent(LessonChangeGroup group, MasterData masterData, bool isPing)
    {
        DiscordMessageFormatter formatter = new(masterData);
        string message = formatter.Format(group);

        return isPing ? $"@everyone\n{message}" : message;
    }

    private static AllowedMentionsProperties BuildAllowedMentions(bool isPing)
    {
        return isPing ? AllowedMentionsProperties.None.WithEveryone(true) : AllowedMentionsProperties.None;
    }
}
