using System.ComponentModel.DataAnnotations;

namespace Sentyl.Notifications.Discord;

public sealed class DiscordOptions
{
    public const string SectionName = "Discord";

    [Required] public ulong ChannelId { get; set; }
    [Required] public string BotToken { get; set; } = "";
}
