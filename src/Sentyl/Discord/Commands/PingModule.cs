using NetCord.Services.ApplicationCommands;

namespace Sentyl.Discord.Commands;

public class PingModule : ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand("ping", "Answers with Pong")]
    public string Ping() => "Pong 🏓";
}
