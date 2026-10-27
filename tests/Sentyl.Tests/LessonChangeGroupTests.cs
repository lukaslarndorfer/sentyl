using AwesomeAssertions;

using Sentyl.ChangeDetection;
using Sentyl.Notifications;
using Sentyl.Tests.Helpers;
using Sentyl.Timetable;

using Xunit;

namespace Sentyl.Tests;

public class LessonChangeGroupTests
{
    [Fact]
    public void GroupByDateAndStart_SameTime_ReturnsCorrectGroups()
    {
        Lesson firstLesson = TestLessons.Create(id: 123, rooms: [2]);
        Lesson secondLesson = TestLessons.Create(id: 124, rooms: [3]);

        Lesson currentFirst = TestLessons.Create(id: 123, rooms: [4]);
        Lesson currentSecond = TestLessons.Create(id: 124, rooms: [5]);

        IReadOnlyList<LessonChange> changes = LessonChangeDetector.DetectChanges([firstLesson, secondLesson], [currentFirst, currentSecond]);
        var grouped = LessonChangeGroup.GroupByDateAndStart(changes);

        grouped.Count.Should().Be(1);
        grouped[0].Changes.Count.Should().Be(2);
    }

    [Fact]
    public void GroupByDateAndStart_DifferentTime_ReturnsCorrectGroups()
    {
        Lesson firstLesson = TestLessons.Create(id: 123, rooms: [2]);

        var laterStart = firstLesson.Start.AddHours(1);
        var laterEnd = firstLesson.End.AddHours(1);
        Lesson secondLesson = TestLessons.Create(id: 124, rooms: [3], start: laterStart, end: laterEnd);

        Lesson currentFirst = TestLessons.Create(id: 123, rooms: [4]);
        Lesson currentSecond = TestLessons.Create(id: 124, rooms: [5], start: laterStart, end: laterEnd);

        IReadOnlyList<LessonChange> changes = LessonChangeDetector.DetectChanges([firstLesson, secondLesson], [currentFirst, currentSecond]);
        var grouped = LessonChangeGroup.GroupByDateAndStart(changes);

        grouped.Should().HaveCount(2);
        grouped[0].Changes.Should().ContainSingle().Which.Current.Id.Should().Be(123); // sorting by time
        grouped[1].Changes.Should().ContainSingle().Which.Current.Id.Should().Be(124);
    }
}
