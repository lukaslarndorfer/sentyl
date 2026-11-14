namespace Sentyl.Persistence;

public class GuildConfig
{
    public ulong GuildId { get; set; }
    public ulong ChannelId { get; set; }
    public required string UntisHost { get; set; }
    public required string UntisSchool { get; set; }
    public required string UntisUser { get; set; }
    public required string UntisPasswordEncrypted { get; set; }
    public required bool Enabled { get; set; }
}
