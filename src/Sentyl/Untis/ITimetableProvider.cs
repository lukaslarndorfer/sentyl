namespace Sentyl.Untis;

public interface ITimetableProvider
{
    public Task<List<TimetableEntry>> GetTimetableAsync(
        DateOnly start,
        DateOnly end,
        CancellationToken cancellationToken);
}
