namespace UntisSentinel.Timetable;

public interface ITimetableStateStore
{
    public Task<IReadOnlyList<Lesson>?> LoadAsync(CancellationToken cancellationToken);

    public Task SaveAsync(IReadOnlyList<Lesson> lessons, CancellationToken cancellationToken);
}
