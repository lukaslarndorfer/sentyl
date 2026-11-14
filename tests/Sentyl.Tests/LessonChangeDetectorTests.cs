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
        const int PreviousTeacherId = 2;
        const int NewTeacherId = 3;
        var previous = TestLessons.Create(teachers: [PreviousTeacherId]);
        var current = TestLessons.Create(teachers: [NewTeacherId], substitutedTeacherIds: [PreviousTeacherId]);

        IReadOnlyList<LessonChange> changes = LessonChangeDetector.DetectChanges([previous], [current]);

        var change = changes.Should().ContainSingle().Subject;
        change.Type.Should().Be(LessonChangeType.TeacherSubstituted);
        change.Previous.Should().BeSameAs(previous);
        change.Current.Should().BeSameAs(current);
    }

    [Fact]
    public void DetectChanges_SubstitutionDisappears_ReturnsTeacherSubstitutionWithdrawn()
    {
        const int PreviousTeacherId = 2;
        const int NewTeacherId = 3;
        var previous = TestLessons.Create(teachers: [NewTeacherId], substitutedTeacherIds: [PreviousTeacherId]);
        var current = TestLessons.Create(teachers: [PreviousTeacherId]);

        IReadOnlyList<LessonChange> changes = LessonChangeDetector.DetectChanges([previous], [current]);

        var change = changes.Should().ContainSingle().Subject;
        change.Type.Should().Be(LessonChangeType.TeacherSubstitutionWithdrawn);
        change.Previous.Should().BeSameAs(previous);
        change.Current.Should().BeSameAs(current);
    }

    [Fact]
    public void DetectChanges_RoomChanged_ReturnsRoomChanged()
    {
        const int PreviousRoomId = 2;
        const int NewRoomId = 3;
        var previous = TestLessons.Create(rooms: [PreviousRoomId]);
        var current = TestLessons.Create(rooms: [NewRoomId]);

        IReadOnlyList<LessonChange> changes = LessonChangeDetector.DetectChanges([previous], [current]);

        var change = changes.Should().ContainSingle().Subject;
        change.Type.Should().Be(LessonChangeType.RoomChanged);
        change.Previous.Should().BeSameAs(previous);
        change.Current.Should().BeSameAs(current);
    }

    [Fact]
    public void DetectChanges_RoomAndTeacherChanged_ReturnsTwoChanges()
    {
        const int PreviousRoomId = 2;
        const int NewRoomId = 3;
        const int PreviousTeacherId = 4;
        const int NewTeacherId = 5;

        var previous = TestLessons.Create(rooms: [PreviousRoomId], teachers: [PreviousTeacherId]);
        var current = TestLessons.Create(rooms: [NewRoomId], teachers: [NewTeacherId], substitutedTeacherIds: [PreviousTeacherId]);

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
        const int PreviousTeacherId = 2;
        const int NewTeacherId = 3;

        var previous = TestLessons.Create(teachers: [NewTeacherId], substitutedTeacherIds: [PreviousTeacherId]);
        var current = TestLessons.Create(teachers: [NewTeacherId], substitutedTeacherIds: [PreviousTeacherId]);

        IReadOnlyList<LessonChange> changes = LessonChangeDetector.DetectChanges([previous], [current]);

        changes.Should().BeEmpty();
    }

    [Fact]
    public void DetectChanges_SameRoomsInDifferentOrder_ReturnsEmpty()
    {
        const int RoomA = 2;
        const int RoomB = 3;

        var previous = TestLessons.Create(rooms: [RoomA, RoomB]);
        var current = TestLessons.Create(rooms: [RoomB, RoomA]);

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
        const int PreviousTeacherId = 2;
        const int NewTeacherId = 3;
        const int AnotherNewTeacherId = 4;

        var previous = TestLessons.Create(teachers: [NewTeacherId], substitutedTeacherIds: [PreviousTeacherId]);
        var current = TestLessons.Create(teachers: [AnotherNewTeacherId], substitutedTeacherIds: [PreviousTeacherId]);

        IReadOnlyList<LessonChange> changes = LessonChangeDetector.DetectChanges([previous], [current]);

        var change = changes.Should().ContainSingle().Subject;
        change.Type.Should().Be(LessonChangeType.TeacherSubstitutionChanged);
        change.Previous.Should().BeSameAs(previous);
        change.Current.Should().BeSameAs(current);
    }

    [Fact]
    public void DetectChanges_TeacherSubstitutionChangedButSameTeachers_ReturnsEmpty()
    {
        const int PreviousTeacherId = 2;
        const int NewTeacherId = 3;
        const int AnotherNewTeacherId = 4;

        var previous = TestLessons.Create(teachers: [NewTeacherId, AnotherNewTeacherId], substitutedTeacherIds: [PreviousTeacherId]);
        var current = TestLessons.Create(teachers: [AnotherNewTeacherId, NewTeacherId], substitutedTeacherIds: [PreviousTeacherId]);

        IReadOnlyList<LessonChange> changes = LessonChangeDetector.DetectChanges([previous], [current]);

        changes.Should().BeEmpty();
    }
}
