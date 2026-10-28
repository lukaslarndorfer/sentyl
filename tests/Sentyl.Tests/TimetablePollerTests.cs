using AwesomeAssertions;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;

using Sentyl.ChangeDetection;
using Sentyl.Tests.Helpers;
using Sentyl.Timetable;
using Sentyl.Untis;

namespace Sentyl.Tests;

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
            notificationSender,
            new FakeTimeProvider());

        await timetablePoller.PollOnceAsync(CancellationToken.None);
        var stored = await timetableStateStore.LoadAsync(CancellationToken.None);

        notificationSender.SentGroups.Should().BeEmpty();
        stored.Should().NotBeNull();
        stored.Should().ContainSingle().Subject.Id.Should().Be(1);
    }

    [Fact]
    public async Task PollOnceAsync_WhenLessonIsCancelled_SendsCancellationNotification()
    {
        TimetableEntry regularEntry = new(
            Id: 1,
            Date: 20261019,
            StartTime: 800,
            EndTime: 850,
            Code: null,
            ActivityType: "Unterricht",
            Classes: [new ElementRef(3065, null)],
            Teachers: [new ElementRef(14, null)],
            Subjects: [new ElementRef(35, null)],
            Rooms: [new ElementRef(42, null)]);

        TimetableEntry cancelledEntry = new(
            Id: 1,
            Date: 20261019,
            StartTime: 800,
            EndTime: 850,
            Code: "cancelled",
            ActivityType: "Unterricht",
            Classes: [new ElementRef(3065, null)],
            Teachers: [new ElementRef(14, null)],
            Subjects: [new ElementRef(35, null)],
            Rooms: [new ElementRef(42, null)]);

        InMemoryTimetableStateStore timetableStateStore = new();
        RecordingNotificationSender notificationSender = new();
        IOptions<PollingOptions> pollingOptions = Options.Create(new PollingOptions());
        FakeTimetableProvider timetableProvider = new(
            [cancelledEntry]);

        TimetablePoller timetablePoller = new(
            NullLogger<TimetablePoller>.Instance,
            pollingOptions,
            timetableStateStore,
            timetableProvider,
            notificationSender,
            new FakeTimeProvider());

        await timetableStateStore.SaveAsync(
            [regularEntry.ToLesson()],
            CancellationToken.None);
        await timetablePoller.PollOnceAsync(CancellationToken.None);

        var group = notificationSender.SentGroups.Should().ContainSingle().Subject;
        var change = group.Changes.Should().ContainSingle().Subject;
        change.Type.Should().Be(LessonChangeType.Cancelled);
    }

    [Fact]
    public async Task PollOnceAsync_WhenNotificationFails_DoesNotSaveNewState()
    {
        TimetableEntry regularEntry = new(
            Id: 1,
            Date: 20261019,
            StartTime: 800,
            EndTime: 850,
            Code: null,
            ActivityType: "Unterricht",
            Classes: [new ElementRef(3065, null)],
            Teachers: [new ElementRef(14, null)],
            Subjects: [new ElementRef(35, null)],
            Rooms: [new ElementRef(42, null)]);

        TimetableEntry cancelledEntry = regularEntry with { Code = "cancelled" };

        InMemoryTimetableStateStore timetableStateStore = new();
        ThrowingNotificationSender notificationSender = new();
        IOptions<PollingOptions> pollingOptions = Options.Create(new PollingOptions());
        FakeTimetableProvider timetableProvider = new(
            [cancelledEntry]);

        TimetablePoller timetablePoller = new(
            NullLogger<TimetablePoller>.Instance,
            pollingOptions,
            timetableStateStore,
            timetableProvider,
            notificationSender,
            new FakeTimeProvider());

        await timetableStateStore.SaveAsync(
            [regularEntry.ToLesson()],
            CancellationToken.None);
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => timetablePoller.PollOnceAsync(CancellationToken.None));

        // previous state must remain unchanged when notification sending fails
        var stored = await timetableStateStore.LoadAsync(CancellationToken.None);
        stored.Should().ContainSingle().Subject.Status.Should().Be(LessonStatus.Regular);
    }
}
