using System.ComponentModel.DataAnnotations;

namespace Sentyl.Notifications.Discord;

public sealed class DiscordOptions
{
    public const string SectionName = "Discord";

    [Required] public string WebhookUrl { get; set; } = "";
}
