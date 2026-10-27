namespace Sentyl.Timetable;

public sealed class InMemoryTimetableStateStore : ITimetableStateStore
{
    private IReadOnlyList<Lesson>? _lessons;

    public Task<IReadOnlyList<Lesson>?> LoadAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_lessons);
    }

    public Task SaveAsync(IReadOnlyList<Lesson> lessons, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _lessons = lessons;
        return Task.CompletedTask;
    }
}
