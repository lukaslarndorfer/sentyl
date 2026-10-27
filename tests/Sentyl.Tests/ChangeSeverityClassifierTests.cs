using AwesomeAssertions;

using Microsoft.Extensions.Time.Testing;

using Sentyl.ChangeDetection;
using Sentyl.Tests.Helpers;
using Sentyl.Timetable;

namespace Sentyl.Tests;

public class ChangeSeverityClassifierTests
{

    [Theory]
    // "now" values based on testlesson default values (2026-09-28; start 08:50, end 09:40)
    [InlineData("2026-09-28 09:00", NotificationSeverity.Ping)]
    [InlineData("2026-09-27 11:00", NotificationSeverity.Ping)]
    [InlineData("2026-09-27 07:00", NotificationSeverity.Info)]
    [InlineData("2026-09-28 10:00", NotificationSeverity.Info)]
    [InlineData("2026-09-28 09:40", NotificationSeverity.Info)]
    [InlineData("2026-09-27 08:50", NotificationSeverity.Info)]
    public void Classify_ReturnsSeverityBasedOnTimeWindow(string rawNow, NotificationSeverity expectedSeverity)
    {
        DateTime now = DateTime.Parse(rawNow);
        var fakeTime = CreateFakeTimeProvider(now);

        Lesson lesson = TestLessons.Create();
        var change = new LessonChange(LessonChangeType.Cancelled, lesson, lesson);

        NotificationSeverity severity = ChangeSeverityClassifier.Classify(change, fakeTime);
        severity.Should().Be(expectedSeverity);

    }

    [Theory]
    [InlineData(LessonChangeType.Cancelled, NotificationSeverity.Ping)]
    [InlineData(LessonChangeType.CancellationWithdrawn, NotificationSeverity.Ping)]
    [InlineData(LessonChangeType.TeacherSubstituted, NotificationSeverity.Ping)]
    [InlineData(LessonChangeType.TeacherSubstitutionWithdrawn, NotificationSeverity.Ping)]
    [InlineData(LessonChangeType.TeacherSubstitutionChanged, NotificationSeverity.Info)]
    [InlineData(LessonChangeType.RoomChanged, NotificationSeverity.Ping)]
    [InlineData(LessonChangeType.LessonAdded, NotificationSeverity.Ping)]
    [InlineData(LessonChangeType.BecameIrregular, NotificationSeverity.Ping)]
    public void Classify_ReturnsSeverityBasedOnChangeType(LessonChangeType lessonChangeType, NotificationSeverity expectedSeverity)
    {
        var now = new DateTime(2026, 9, 28, 9, 0, 0);
        var fakeTime = CreateFakeTimeProvider(now);

        Lesson lesson = TestLessons.Create();
        var change = new LessonChange(lessonChangeType, lesson, lesson);

        NotificationSeverity result = ChangeSeverityClassifier.Classify(change, fakeTime);

        result.Should().Be(expectedSeverity);
    }

    [Fact]
    public void Classify_ThrowsForUnclassifiedChangeType()
    {
        var now = new DateTime(2026, 9, 28, 9, 0, 0);
        var fakeTime = CreateFakeTimeProvider(now);

        var lesson = TestLessons.Create();
        var change = new LessonChange((LessonChangeType)999, lesson, lesson);
        Assert.Throws<ArgumentOutOfRangeException>(() => ChangeSeverityClassifier.Classify(change, fakeTime));
    }

    private static FakeTimeProvider CreateFakeTimeProvider(DateTime localNow)
    {
        var fakeTime = new FakeTimeProvider(new DateTimeOffset(localNow, TimeSpan.Zero));
        fakeTime.SetLocalTimeZone(TimeZoneInfo.Utc);
        return fakeTime;
    }
}
