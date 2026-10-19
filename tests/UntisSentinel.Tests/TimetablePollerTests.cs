using AwesomeAssertions;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using UntisSentinel.Tests.Helpers;
using UntisSentinel.Timetable;
using UntisSentinel.Untis;

namespace UntisSentinel.Tests;

public class TimetablePollerTests
{
    [Fact]
    public async Task PollOnceAsync_WhenStateIsEmpty_DoesNotSendNotification()
    {
        InMemoryTimetableStateStore timetableStateStore = new();
        RecordingNotificationSender notificationSender = new();

        IOptions<PollingOptions> pollingOptions = Options.Create(new PollingOptions());
        FakeTimetableProvider timetableProvider = new(
            [
                new TimetableEntry(
                    Id: 1,
                    Date: 20261019,
                    StartTime: 800,
                    EndTime: 850,
                    Code: null,
                    ActivityType: "Unterricht",
                    Classes: [new ElementRef(3065, null)],
                    Teachers: [new ElementRef(14, null)],
                    Subjects: [new ElementRef(35, null)],
                    Rooms: [new ElementRef(42, null)])
            ]);

        TimetablePoller timetablePoller = new(
            NullLogger<TimetablePoller>.Instance,
            pollingOptions,
            timetableStateStore,
            timetableProvider,
            notificationSender);

        await timetablePoller.PollOnceAsync(CancellationToken.None);
        var stored = await timetableStateStore.LoadAsync(CancellationToken.None);

        notificationSender.SentGroups.Should().BeEmpty();
        stored.Should().NotBeNull();
        stored.Should().ContainSingle().Subject.Id.Should().Be(1);
    }
}
