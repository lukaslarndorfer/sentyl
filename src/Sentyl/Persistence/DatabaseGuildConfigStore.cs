using Microsoft.EntityFrameworkCore;

namespace Sentyl.Persistence;

public sealed class DatabaseGuildConfigStore(IDbContextFactory<DatabaseContext> factory) : IGuildConfigStore
{
    public async Task<GuildConfig?> GetAsync(ulong guildId, CancellationToken cancellationToken)
    {
        await using var db = await factory.CreateDbContextAsync(cancellationToken);

        return await db.GuildConfigs
                       .AsNoTracking()
                       .FirstOrDefaultAsync(c => c.GuildId == guildId, cancellationToken);
    }

    public async Task<IReadOnlyList<GuildConfig>> GetEnabledAsync(CancellationToken cancellationToken)
    {
        await using var db = await factory.CreateDbContextAsync(cancellationToken);

        return await db.GuildConfigs
                       .AsNoTracking()
                       .Where(c => c.Enabled)
                       .ToListAsync(cancellationToken);
    }

    public async Task UpsertAsync(GuildConfig config, CancellationToken cancellationToken)
    {
        await using var db = await factory.CreateDbContextAsync(cancellationToken);

        bool exists = await db.GuildConfigs.AnyAsync(c => c.GuildId == config.GuildId, cancellationToken);

        if (exists)
        {
            db.GuildConfigs.Update(config);
        }
        else
        {
            db.GuildConfigs.Add(config);
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(ulong guildId, CancellationToken cancellationToken)
    {
        await using var db = await factory.CreateDbContextAsync(cancellationToken);

        await db.GuildConfigs
                .Where(c => c.GuildId == guildId)
                .ExecuteDeleteAsync(cancellationToken);
    }
}
