using System.ComponentModel.DataAnnotations;

using AwesomeAssertions;

using Sentyl.ChangeDetection;
using Sentyl.Notifications;
using Sentyl.Notifications.Discord;
using Sentyl.Tests.Helpers;
using Sentyl.Timetable;
using Sentyl.Untis;

namespace Sentyl.Tests;

public class DiscordMessageFormatterTests
{
    private readonly Teacher _teacher = new(1, "John", "Doe", "JODO");
    private readonly Teacher _firstSubstitute = new(2, "Jane", "Doe", "JADO");
    private readonly Teacher _secondSubstitute = new(3, "Joe", "Smith", "JOSM");
    private readonly Subject _subject = new(1, "MA", "Mathematik", null, null);
    private readonly Room _room = new(1, "P-1", "Physiksaal-1");
    private readonly Room _newRoom = new(2, "P-2", "Physiksaal-2");
    private readonly DiscordMessageFormatter _formatter;
    public static TheoryData<LessonChangeType> AllChangeTypes => new(Enum.GetValues<LessonChangeType>());

    public DiscordMessageFormatterTests()
    {
        MasterData masterData = new(classes: [], teachers: [_teacher, _firstSubstitute, _secondSubstitute], subjects: [_subject], rooms: [_room, _newRoom]);
        _formatter = new(masterData);
    }

    [Fact]
    public void Format_Cancelled_ContainsCancellationLine()
    {
        Lesson lesson = TestLessons.Create(teachers: [_teacher.Id], subjects: [_subject.Id], rooms: [_room.Id]);

        var result = FormatChange(LessonChangeType.Cancelled, lesson, lesson);

        result.Should().Contain($"{_subject.Abbreviation} ({_teacher.Abbreviation}) entfällt!");
    }

    [Fact]
    public void Format_CancellationWithdrawn_ContainsWithdrawnLine()
    {
        Lesson previousLesson = TestLessons.Create(teachers: [_teacher.Id], subjects: [_subject.Id], rooms: [_room.Id], status: LessonStatus.Cancelled);
        Lesson newLesson = TestLessons.Create(teachers: [_teacher.Id], subjects: [_subject.Id], rooms: [_room.Id], status: LessonStatus.Regular);

        var result = FormatChange(LessonChangeType.CancellationWithdrawn, previousLesson, newLesson);

        result.Should().Contain($"{_subject.Abbreviation} ({_teacher.Abbreviation}) findet doch statt!");
    }

    [Fact]
    public void Format_TeacherSubstituted_ContainsSubstitutionLine()
    {
        Lesson previousLesson = TestLessons.Create(teachers: [_teacher.Id], subjects: [_subject.Id], rooms: [_room.Id]);
        Lesson currentLesson = TestLessons.Create(teachers: [_firstSubstitute.Id], subjects: [_subject.Id], rooms: [_room.Id], substitutedTeacherIds: [_teacher.Id]);

        var result = FormatChange(LessonChangeType.TeacherSubstituted, previousLesson, currentLesson);

        result.Should().Contain($"{_subject.Abbreviation}: Vertretung durch {_firstSubstitute.Abbreviation}");
    }

    [Fact]
    public void Format_TeacherSubstitutionWithdrawn_ContainsWithdrawnLine()
    {
        Lesson previousLesson = TestLessons.Create(teachers: [_firstSubstitute.Id], subjects: [_subject.Id], rooms: [_room.Id], substitutedTeacherIds: [_teacher.Id]);
        Lesson currentLesson = TestLessons.Create(teachers: [_teacher.Id], subjects: [_subject.Id], rooms: [_room.Id]);

        var result = FormatChange(LessonChangeType.TeacherSubstitutionWithdrawn, previousLesson, currentLesson);

        result.Should().Contain($"{_subject.Abbreviation}: Vertretung widerrufen! Aktuell: {_teacher.Abbreviation}");
    }

    [Fact]
    public void Format_TeacherSubstitutionChanged_ContainsChangeLine()
    {
        Lesson previousLesson = TestLessons.Create(teachers: [_firstSubstitute.Id], subjects: [_subject.Id], rooms: [_room.Id], substitutedTeacherIds: [_teacher.Id]);
        Lesson currentLesson = TestLessons.Create(teachers: [_secondSubstitute.Id], subjects: [_subject.Id], rooms: [_room.Id], substitutedTeacherIds: [_teacher.Id]);

        var result = FormatChange(LessonChangeType.TeacherSubstitutionChanged, previousLesson, currentLesson);

        result.Should().Contain($"{_subject.Abbreviation}: Vertretung geändert! Aktuell: {_secondSubstitute.Abbreviation}; Davor: {_firstSubstitute.Abbreviation}");
    }

    [Fact]
    public void Format_RoomChanged_ReturnsChangeLine()
    {
        Lesson previousLesson = TestLessons.Create(teachers: [_teacher.Id], subjects: [_subject.Id], rooms: [_room.Id]);
        Lesson currentLesson = TestLessons.Create(teachers: [_teacher.Id], subjects: [_subject.Id], rooms: [_newRoom.Id]);

        var result = FormatChange(LessonChangeType.RoomChanged, previousLesson, currentLesson);

        result.Should().Contain($"{_subject.Abbreviation}: Raumwechsel: Von {_room.Name} zu {_newRoom.Name}");
    }

    [Fact]
    public void Format_LessonAdded_ReturnsAddLine()
    {
        Lesson currentLesson = TestLessons.Create(teachers: [_teacher.Id], subjects: [_subject.Id], rooms: [_room.Id]);

        var result = FormatChange(LessonChangeType.LessonAdded, null, currentLesson);

        result.Should().Contain($"{_subject.Abbreviation}: Stunde hinzugefügt!");
    }

    [Fact]
    public void Format_BecameIrregular_ReturnsIrregularLine()
    {
        Lesson previousLesson = TestLessons.Create(teachers: [_teacher.Id], subjects: [_subject.Id], rooms: [_room.Id]);
        Lesson currentLesson = TestLessons.Create(teachers: [_teacher.Id], subjects: [_subject.Id], rooms: [_room.Id], status: LessonStatus.Irregular);

        var result = FormatChange(LessonChangeType.BecameIrregular, previousLesson, currentLesson);

        result.Should().Contain($"{_subject.Abbreviation} findet außerplanmäßig statt!");
    }

    [Fact]
    public void Format_UnknownId_ReturnsQuestionMark()
    {
        Lesson lesson = TestLessons.Create(teachers: [99], subjects: [_subject.Id], rooms: [_room.Id]);

        var result = FormatChange(LessonChangeType.Cancelled, lesson, lesson);

        result.Should().Contain($"{_subject.Abbreviation} (?) entfällt!"); // question mark = missing value
    }

    [Fact]
    public void Format_NoId_ReturnsDash()
    {
        Lesson lesson = TestLessons.Create(subjects: [_subject.Id], rooms: [_room.Id]);

        var result = FormatChange(LessonChangeType.Cancelled, lesson, lesson);

        result.Should().Contain($"{_subject.Abbreviation} (-) entfällt!"); // dash = empty list
    }

    [Fact]
    public void Format_MultipleTeachers_ReturnsMultipleTeachers()
    {
        Lesson lesson = TestLessons.Create(teachers: [_teacher.Id, _firstSubstitute.Id], subjects: [_subject.Id], rooms: [_room.Id]);

        var result = FormatChange(LessonChangeType.Cancelled, lesson, lesson);

        result.Should().Contain($"{_subject.Abbreviation} (JODO, JADO) entfällt!");
    }

    [Fact]
    public void Format_MissingPrevious_ThrowsInvalidOperationException()
    {
        Lesson lesson = TestLessons.Create(teachers: [_teacher.Id], subjects: [_subject.Id], rooms: [_room.Id]);

        Action act = () => FormatChange(LessonChangeType.TeacherSubstitutionChanged, null, lesson);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Format_Cancelled_ContainsCorrectHeading()
    {
        Lesson lesson = TestLessons.Create(teachers: [_teacher.Id], subjects: [_subject.Id], rooms: [_room.Id]);

        var result = FormatChange(LessonChangeType.Cancelled, lesson, lesson);

        result.Should().Contain("**Mo 28.09. – 08:50**"); // default value from TestLessons
    }

    [Theory]
    [MemberData(nameof(AllChangeTypes))]
    public void Format_AllChangeTypes_DoesNotThrow(LessonChangeType type)
    {
        Lesson lesson = TestLessons.Create(teachers: [_teacher.Id], subjects: [_subject.Id], rooms: [_room.Id]);

        var result = FormatChange(type, lesson, lesson);

        result.Should().NotBeNullOrWhiteSpace();
    }

    // builds a group with a single change and formats it
    private string FormatChange(LessonChangeType type, Lesson? previous, Lesson current)
    {
        LessonChange change = new(type, previous, current);
        LessonChangeGroup group = new(current.Date, current.Start, [change]);

        return _formatter.Format(group);
    }
}
