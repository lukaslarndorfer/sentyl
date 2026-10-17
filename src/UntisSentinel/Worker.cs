using Microsoft.Extensions.Options;

using UntisSentinel.Timetable;
using UntisSentinel.Untis;

namespace UntisSentinel;

public class Worker(
    MasterDataLoader masterDataLoader,
    IOptions<PollingOptions> pollingOptions,
    TimetablePoller timetablePoller) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        MasterData masterData = await masterDataLoader.LoadAsync(stoppingToken);

        // periodic timer ticks at a fixed rate, so the run time doesn't add to the interval (no drift like Task.Delay has)
        using PeriodicTimer timer = new(pollingOptions.Value.Interval);

        // first run right away, then every interval
        do
        {
            await timetablePoller.PollOnceAsync(stoppingToken);
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
