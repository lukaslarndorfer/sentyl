using Microsoft.Extensions.Options;

using NetCord.Rest;

using Polly.Timeout;

using Sentyl.ChangeDetection;
using Sentyl.Notifications;
using Sentyl.Untis;

namespace Sentyl.Timetable;

public sealed class TimetablePoller(
    ILogger<TimetablePoller> logger,
    IOptions<PollingOptions> pollingOptions,
    ITimetableStateStore timetableStateStore,
    ITimetableProvider client,
    INotificationSender notificationSender,
    TimeProvider timeProvider)
{
    public async Task PollOnceAsync(CancellationToken cancellationToken)
    {
        try
        {
            var previousLessons = await timetableStateStore.LoadAsync(cancellationToken) ?? [];

            DateOnly today = DateOnly.FromDateTime(DateTime.Now);
            DateOnly start = today;
            DateOnly end = today.AddDays(pollingOptions.Value.DaysToFetch - 1);

            List<TimetableEntry> entries = await client.GetTimetableAsync(start, end, cancellationToken);
            List<Lesson> current = [.. entries.Select(tte => tte.ToLesson())];

            logger.LogInformation("Received {Count} lessons", current.Count);

            IReadOnlyList<LessonChange> changes = LessonChangeDetector.DetectChanges(previousLessons, current);
            IReadOnlyList<LessonChangeGroup> groups = LessonChangeGroup.GroupByDateAndStart(changes, timeProvider);
            foreach (LessonChangeGroup group in groups)
            {
                await notificationSender.SendAsync(group, cancellationToken);
            }

            await timetableStateStore.SaveAsync(current, cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException or UntisClientException or TimeoutRejectedException or RestException) // unexpected errors should still stop execution
        {
            logger.LogError(ex, "Failed to poll timetable, skipping this iteration");
        }
    }
}
