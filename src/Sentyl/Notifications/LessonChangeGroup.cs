using Sentyl.ChangeDetection;

namespace Sentyl.Notifications;

// parallel running lessons
public sealed record LessonChangeGroup(DateOnly Date, TimeOnly Start, IReadOnlyList<LessonChange> Changes, NotificationSeverity Severity)
{
    public static IReadOnlyList<LessonChangeGroup> GroupByDateAndStart(IReadOnlyList<LessonChange> changes, TimeProvider timeProvider)
    {
        var grouped = changes.GroupBy(c => (c.Current.Date, c.Current.Start));
        return [.. grouped
        .Select(group => new LessonChangeGroup(
            group.Key.Date,
            group.Key.Start,
            [..group],
            group.Any(change => ChangeSeverityClassifier.Classify(change, timeProvider) == NotificationSeverity.Ping)
                ? NotificationSeverity.Ping
                : NotificationSeverity.Info))
        .OrderBy(group => group.Date)
        .ThenBy(group => group.Start)];
    }
}
