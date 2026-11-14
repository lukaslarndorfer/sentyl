namespace Sentyl.Timetable;

public interface ITimetableStateStore
{
    public Task<IReadOnlyList<Lesson>?> LoadAsync(ulong guildId, CancellationToken cancellationToken);

    public Task SaveAsync(ulong guildId, IReadOnlyList<Lesson> lessons, CancellationToken cancellationToken);
}
