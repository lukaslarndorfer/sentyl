using UntisSentinel.ChangeDetection;

namespace UntisSentinel.Notifications;

// parallel running lessons
public sealed record LessonChangeGroup(DateOnly Date, TimeOnly Start, IReadOnlyList<LessonChange> Changes)
{
    public static IReadOnlyList<LessonChangeGroup> GroupByDateAndStart(IReadOnlyList<LessonChange> changes)
    {
        var grouped = changes.GroupBy(c => (c.Current.Date, c.Current.Start));
        return [.. grouped
        .Select(group => new LessonChangeGroup(
            group.Key.Date,
            group.Key.Start,
            [..group]))
        .OrderBy(group => group.Date)
        .ThenBy(group => group.Start)];
    }
}
