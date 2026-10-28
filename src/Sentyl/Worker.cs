using Microsoft.Extensions.Options;

using Sentyl.Timetable;

namespace Sentyl;

public class Worker(
    IOptions<PollingOptions> pollingOptions,
    TimetablePoller timetablePoller) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // periodic timer ticks at a fixed rate, so the run time doesn't add to the interval (no drift like Task.Delay has)
        using PeriodicTimer timer = new(pollingOptions.Value.Interval);

        // first run right away, then every interval
        do
        {
            await timetablePoller.PollOnceAsync(stoppingToken);
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
