namespace Sentyl.Timetable;

public sealed class InMemoryTimetableStateStore : ITimetableStateStore
{
    private readonly Dictionary<ulong, IReadOnlyList<Lesson>> _states = [];

    public Task<IReadOnlyList<Lesson>?> LoadAsync(ulong guildId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_states.GetValueOrDefault(guildId));
    }

    public Task SaveAsync(ulong guildId, IReadOnlyList<Lesson> lessons, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _states[guildId] = lessons;
        return Task.CompletedTask;
    }
}
