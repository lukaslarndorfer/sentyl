namespace Sentyl.Persistence;

public interface IGuildConfigStore
{
    public Task<GuildConfig?> GetAsync(ulong guildId, CancellationToken cancellationToken);
    public Task<IReadOnlyList<GuildConfig>> GetEnabledAsync(CancellationToken cancellationToken);
    public Task UpsertAsync(GuildConfig config, CancellationToken cancellationToken);
    public Task DeleteAsync(ulong guildId, CancellationToken cancellationToken);
}
