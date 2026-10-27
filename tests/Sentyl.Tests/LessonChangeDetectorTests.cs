using AwesomeAssertions;

using Sentyl.ChangeDetection;
using Sentyl.Tests.Helpers;
using Sentyl.Timetable;

namespace Sentyl.Tests;

public class LessonChangeDetectorTests
{

    [Fact]
    public void DetectChanges_NothingChanged_ReturnsEmpty()
    {
        var previous = TestLessons.Create();
        var current = TestLessons.Create();

        IReadOnlyList<LessonChange> changes = LessonChangeDetector.DetectChanges([previous], [current]);

        changes.Should().BeEmpty();
    }

    [Fact]
    public void DetectChanges_RegularToCancelled_ReturnsCancelled()
    {
        var previous = TestLessons.Create();
        var current = TestLessons.Create(status: LessonStatus.Cancelled);

        IReadOnlyList<LessonChange> changes = LessonChangeDetector.DetectChanges([previous], [current]);

        var change = changes.Should().ContainSingle().Subject;
        change.Type.Should().Be(LessonChangeType.Cancelled);
        change.Previous.Should().BeSameAs(previous);
        change.Current.Should().BeSameAs(current);
    }

    [Fact]
    public void DetectChanges_CancelledToRegular_ReturnsCancellationWithdrawn()
    {
        var previous = TestLessons.Create(status: LessonStatus.Cancelled);
        var current = TestLessons.Create(status: LessonStatus.Regular);

        IReadOnlyList<LessonChange> changes = LessonChangeDetector.DetectChanges([previous], [current]);

        var change = changes.Should().ContainSingle().Subject;
        change.Type.Should().Be(LessonChangeType.CancellationWithdrawn);
        change.Previous.Should().BeSameAs(previous);
        change.Current.Should().BeSameAs(current);
    }

    [Fact]
    public void DetectChanges_SubstitutionAppears_ReturnsTeacherSubstituted()
    {
        const int previousTeacherId = 2;
        const int newTeacherId = 3;
        var previous = TestLessons.Create(teachers: [previousTeacherId]);
        var current = TestLessons.Create(teachers: [newTeacherId], substitutedTeacherIds: [previousTeacherId]);

        IReadOnlyList<LessonChange> changes = LessonChangeDetector.DetectChanges([previous], [current]);

        var change = changes.Should().ContainSingle().Subject;
        change.Type.Should().Be(LessonChangeType.TeacherSubstituted);
        change.Previous.Should().BeSameAs(previous);
        change.Current.Should().BeSameAs(current);
    }

    [Fact]
    public void DetectChanges_SubstitutionDisappears_ReturnsTeacherSubstitutionWithdrawn()
    {
        const int previousTeacherId = 2;
        const int newTeacherId = 3;
        var previous = TestLessons.Create(teachers: [newTeacherId], substitutedTeacherIds: [previousTeacherId]);
        var current = TestLessons.Create(teachers: [previousTeacherId]);

        IReadOnlyList<LessonChange> changes = LessonChangeDetector.DetectChanges([previous], [current]);

        var change = changes.Should().ContainSingle().Subject;
        change.Type.Should().Be(LessonChangeType.TeacherSubstitutionWithdrawn);
        change.Previous.Should().BeSameAs(previous);
        change.Current.Should().BeSameAs(current);
    }

    [Fact]
    public void DetectChanges_RoomChanged_ReturnsRoomChanged()
    {
        const int previousRoomId = 2;
        const int newRoomId = 3;
        var previous = TestLessons.Create(rooms: [previousRoomId]);
        var current = TestLessons.Create(rooms: [newRoomId]);

        IReadOnlyList<LessonChange> changes = LessonChangeDetector.DetectChanges([previous], [current]);

        var change = changes.Should().ContainSingle().Subject;
        change.Type.Should().Be(LessonChangeType.RoomChanged);
        change.Previous.Should().BeSameAs(previous);
        change.Current.Should().BeSameAs(current);
    }

    [Fact]
    public void DetectChanges_RoomAndTeacherChanged_ReturnsTwoChanges()
    {
        const int previousRoomId = 2;
        const int newRoomId = 3;
        const int previousTeacherId = 4;
        const int newTeacherId = 5;

        var previous = TestLessons.Create(rooms: [previousRoomId], teachers: [previousTeacherId]);
        var current = TestLessons.Create(rooms: [newRoomId], teachers: [newTeacherId], substitutedTeacherIds: [previousTeacherId]);

        IReadOnlyList<LessonChange> changes = LessonChangeDetector.DetectChanges([previous], [current]);

        changes.Select(c => c.Type).Should().BeEquivalentTo(
        [LessonChangeType.RoomChanged, LessonChangeType.TeacherSubstituted]);
    }

    [Fact]
    public void DetectChanges_PreviousEmpty_ReturnsEmpty()
    {
        var current = TestLessons.Create();

        IReadOnlyList<LessonChange> changes = LessonChangeDetector.DetectChanges([], [current]);

        changes.Should().BeEmpty();
    }

    [Fact]
    public void DetectChanges_NewLessonWithinPreviousDateRange_ReturnsLessonAdded()
    {
        var first = TestLessons.Create();
        var second = TestLessons.Create(id: 2); // same (default) date, different lesson

        IReadOnlyList<LessonChange> changes = LessonChangeDetector.DetectChanges([first], [first, second]);

        var change = changes.Should().ContainSingle().Subject;
        change.Type.Should().Be(LessonChangeType.LessonAdded);
        change.Current.Should().BeSameAs(second);
        change.Previous.Should().BeNull();
    }

    [Fact]
    public void DetectChanges_NewLessonOutsidePreviousDateRange_ReturnsEmpty()
    {
        var first = TestLessons.Create(date: new DateOnly(2026, 9, 28));
        var second = TestLessons.Create(id: 2, date: new DateOnly(2026, 9, 29));

        IReadOnlyList<LessonChange> changes = LessonChangeDetector.DetectChanges([first], [first, second]);

        changes.Should().BeEmpty();
    }

    [Fact]
    public void DetectChanges_SubstitutionAlreadyPresent_ReturnsEmpty()
    {
        const int previousTeacherId = 2;
        const int newTeacherId = 3;

        var previous = TestLessons.Create(teachers: [newTeacherId], substitutedTeacherIds: [previousTeacherId]);
        var current = TestLessons.Create(teachers: [newTeacherId], substitutedTeacherIds: [previousTeacherId]);

        IReadOnlyList<LessonChange> changes = LessonChangeDetector.DetectChanges([previous], [current]);

        changes.Should().BeEmpty();
    }

    [Fact]
    public void DetectChanges_SameRoomsInDifferentOrder_ReturnsEmpty()
    {
        const int roomA = 2;
        const int roomB = 3;

        var previous = TestLessons.Create(rooms: [roomA, roomB]);
        var current = TestLessons.Create(rooms: [roomB, roomA]);

        IReadOnlyList<LessonChange> changes = LessonChangeDetector.DetectChanges([previous], [current]);

        changes.Should().BeEmpty();
    }

    [Fact]
    public void DetectChanges_MultipleLessonsOnlyOneChanged_ReturnsOnlyThatChange()
    {
        var previousFirst = TestLessons.Create(id: 1);
        var previousSecond = TestLessons.Create(id: 2);
        var previousThird = TestLessons.Create(id: 3);

        var currentFirst = TestLessons.Create(id: 1);
        var currentSecond = TestLessons.Create(id: 2, status: LessonStatus.Cancelled);
        var currentThird = TestLessons.Create(id: 3);

        IReadOnlyList<LessonChange> changes = LessonChangeDetector.
        DetectChanges([previousFirst, previousSecond, previousThird], [currentSecond, currentFirst, currentThird]);

        var change = changes.Should().ContainSingle().Subject;
        change.Type.Should().Be(LessonChangeType.Cancelled);
        change.Previous.Should().BeSameAs(previousSecond);
        change.Current.Should().BeSameAs(currentSecond);
    }

    [Fact]
    public void DetectChanges_RegularToIrregular_ReturnsBecameIrregular()
    {
        var previous = TestLessons.Create(status: LessonStatus.Regular);
        var current = TestLessons.Create(status: LessonStatus.Irregular);

        IReadOnlyList<LessonChange> changes = LessonChangeDetector.DetectChanges([previous], [current]);

        var change = changes.Should().ContainSingle().Subject;
        change.Type.Should().Be(LessonChangeType.BecameIrregular);
        change.Previous.Should().BeSameAs(previous);
        change.Current.Should().BeSameAs(current);
    }

    [Fact]
    public void DetectChanges_CancelledToIrregular_ReturnsBecameIrregularOnly()
    {
        var previous = TestLessons.Create(status: LessonStatus.Cancelled);
        var current = TestLessons.Create(status: LessonStatus.Irregular);

        IReadOnlyList<LessonChange> changes = LessonChangeDetector.DetectChanges([previous], [current]);

        var change = changes.Should().ContainSingle().Subject;
        change.Type.Should().Be(LessonChangeType.BecameIrregular);
        change.Previous.Should().BeSameAs(previous);
        change.Current.Should().BeSameAs(current);
    }

    [Fact]
    public void DetectChanges_AlreadyIrregular_ReturnsEmpty()
    {
        var previous = TestLessons.Create(status: LessonStatus.Irregular);
        var current = TestLessons.Create(status: LessonStatus.Irregular);

        IReadOnlyList<LessonChange> changes = LessonChangeDetector.DetectChanges([previous], [current]);

        changes.Should().BeEmpty();
    }

    [Fact]
    public void DetectChanges_IrregularToRegular_ReturnsEmpty()
    {
        var previous = TestLessons.Create(status: LessonStatus.Irregular);
        var current = TestLessons.Create(status: LessonStatus.Regular);

        IReadOnlyList<LessonChange> changes = LessonChangeDetector.DetectChanges([previous], [current]);

        changes.Should().BeEmpty();
    }

    [Fact]
    public void DetectChanges_IrregularToCancelled_ReturnsCancelled()
    {
        var previous = TestLessons.Create(status: LessonStatus.Irregular);
        var current = TestLessons.Create(status: LessonStatus.Cancelled);

        IReadOnlyList<LessonChange> changes = LessonChangeDetector.DetectChanges([previous], [current]);

        changes.Should().ContainSingle().Subject.Type.Should().Be(LessonChangeType.Cancelled);
    }

    [Fact]
    public void DetectChanges_AlreadyCancelled_ReturnsEmpty()
    {
        var previous = TestLessons.Create(status: LessonStatus.Cancelled);
        var current = TestLessons.Create(status: LessonStatus.Cancelled);

        IReadOnlyList<LessonChange> changes = LessonChangeDetector.DetectChanges([previous], [current]);

        changes.Should().BeEmpty();
    }

    [Fact]
    public void DetectChanges_RegularToUnknown_ReturnsEmpty()
    {
        var previous = TestLessons.Create();
        var current = TestLessons.Create(status: LessonStatus.Unknown);

        IReadOnlyList<LessonChange> changes = LessonChangeDetector.DetectChanges([previous], [current]);

        changes.Should().BeEmpty();
    }

    [Fact]
    public void DetectChanges_CancelledToUnknown_ReturnsEmpty()
    {
        var previous = TestLessons.Create(status: LessonStatus.Cancelled);
        var current = TestLessons.Create(status: LessonStatus.Unknown);

        IReadOnlyList<LessonChange> changes = LessonChangeDetector.DetectChanges([previous], [current]);

        changes.Should().BeEmpty();
    }

    [Fact]
    public void DetectChanges_NoRoomsOnBothSides_ReturnsEmpty()
    {
        var previous = TestLessons.Create(rooms: []);
        var current = TestLessons.Create(rooms: []);

        IReadOnlyList<LessonChange> changes = LessonChangeDetector.DetectChanges([previous], [current]);

        changes.Should().BeEmpty();
    }

    [Fact]
    public void DetectChanges_TeacherSubstitutionChanged_ReturnsTeacherSubstitutionChanged()
    {
        const int previousTeacherId = 2;
        const int newTeacherId = 3;
        const int anotherNewTeacherId = 4;

        var previous = TestLessons.Create(teachers: [newTeacherId], substitutedTeacherIds: [previousTeacherId]);
        var current = TestLessons.Create(teachers: [anotherNewTeacherId], substitutedTeacherIds: [previousTeacherId]);

        IReadOnlyList<LessonChange> changes = LessonChangeDetector.DetectChanges([previous], [current]);

        var change = changes.Should().ContainSingle().Subject;
        change.Type.Should().Be(LessonChangeType.TeacherSubstitutionChanged);
        change.Previous.Should().BeSameAs(previous);
        change.Current.Should().BeSameAs(current);
    }

    [Fact]
    public void DetectChanges_TeacherSubstitutionChangedButSameTeachers_ReturnsEmpty()
    {
        const int previousTeacherId = 2;
        const int newTeacherId = 3;
        const int anotherNewTeacherId = 4;

        var previous = TestLessons.Create(teachers: [newTeacherId, anotherNewTeacherId], substitutedTeacherIds: [previousTeacherId]);
        var current = TestLessons.Create(teachers: [anotherNewTeacherId, newTeacherId], substitutedTeacherIds: [previousTeacherId]);

        IReadOnlyList<LessonChange> changes = LessonChangeDetector.DetectChanges([previous], [current]);

        changes.Should().BeEmpty();
    }
}
