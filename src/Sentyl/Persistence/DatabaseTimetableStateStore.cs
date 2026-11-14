using Microsoft.EntityFrameworkCore;
using Sentyl.Timetable;

namespace Sentyl.Persistence;

public sealed class DatabaseTimetableStateStore(IDbContextFactory<DatabaseContext> factory) : ITimetableStateStore
{
    public async Task<IReadOnlyList<Lesson>?> LoadAsync(ulong guildId, CancellationToken cancellationToken)
    {
        await using var db = await factory.CreateDbContextAsync(cancellationToken);

        return await db.GuildTimetableStates
                       .AsNoTracking()
                       .Where(s => s.GuildId == guildId)
                       .Select(s => s.Lessons)
                       .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task SaveAsync(ulong guildId, IReadOnlyList<Lesson> lessons, CancellationToken cancellationToken)
    {
        await using var db = await factory.CreateDbContextAsync(cancellationToken);

        var state = await db.GuildTimetableStates.FindAsync([guildId], cancellationToken);

        if (state is null)
        {
            db.GuildTimetableStates.Add(new GuildTimetableState { GuildId = guildId, Lessons = [.. lessons] });
        }
        else
        {
            state.Lessons = [.. lessons];
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
