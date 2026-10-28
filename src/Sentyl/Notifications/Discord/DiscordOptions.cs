using System.ComponentModel.DataAnnotations;

namespace Sentyl.Notifications.Discord;

public sealed class DiscordOptions
{
    public const string SectionName = "Discord";

    [Required] public string WebhookId { get; set; } = "";
    [Required] public string WebhookToken { get; set; } = "";
}
