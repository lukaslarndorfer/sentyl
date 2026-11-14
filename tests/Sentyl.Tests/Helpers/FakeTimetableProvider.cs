using Sentyl.Untis;
using Sentyl.Untis.Api;

namespace Sentyl.Tests.Helpers;

internal sealed class FakeTimetableProvider(
    IReadOnlyList<TimetableEntry> entries) : ITimetableProvider
{
    public Task<List<TimetableEntry>> GetTimetableAsync(
        DateOnly start,
        DateOnly end,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(entries.ToList());
    }
}
