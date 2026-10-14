using Microsoft.Extensions.Options;

using Polly.Timeout;

using UntisSentinel.ChangeDetection;
using UntisSentinel.Notifications;
using UntisSentinel.Timetable;
using UntisSentinel.Untis;

namespace UntisSentinel;

public class Worker(ILogger<Worker> logger, MasterDataLoader masterDataLoader, IOptions<PollingOptions> pollingOptions, UntisClient client) : BackgroundService
{
    private List<Lesson> _previousLessons = [];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        MasterData masterData = await masterDataLoader.LoadAsync(stoppingToken);

        // periodic timer ticks at a fixed rate, so the run time doesn't add to the interval (no drift like Task.Delay has)
        using PeriodicTimer timer = new(pollingOptions.Value.Interval);

        // first run right away, then every interval
        do
        {
            await PollOnceAsync(stoppingToken);
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task PollOnceAsync(CancellationToken cancellationToken)
    {
        try
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.Now);
            DateOnly start = today;
            DateOnly end = today.AddDays(pollingOptions.Value.DaysToFetch - 1);

            List<TimetableEntry> entries = await client.GetTimetableAsync(start, end, cancellationToken);
            List<Lesson> current = [.. entries.Select(tte => tte.ToLesson())];

            logger.LogInformation("Received {Count} lessons", current.Count);

            IReadOnlyList<LessonChange> changes = LessonChangeDetector.DetectChanges(_previousLessons, current);
            IReadOnlyList<LessonChangeGroup> groups = LessonChangeGroup.GroupByDateAndStart(changes);
            foreach (LessonChangeGroup group in groups)
            {
                logger.LogInformation(
                    "Lesson changes detected for {Date} at {Start}: {ChangeCount} changes ({ChangeTypes})",
                    group.Date,
                    group.Start,
                    group.Changes.Count,
                    string.Join(", ", group.Changes.Select(change => change.Type)));
            }

            _previousLessons = current;
        }
        catch (Exception ex) when (ex is HttpRequestException or UntisClientException or TimeoutRejectedException) // unexpected errors should still stop execution
        {
            logger.LogError(ex, "Failed to poll timetable, skipping this iteration");
        }
    }
}
