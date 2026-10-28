using Sentyl.Timetable;

namespace Sentyl.Tests.Helpers;

internal static class TestLessons
{
    internal static Lesson Create(
        int id = 1,
        LessonStatus status = LessonStatus.Regular,
        IReadOnlyList<int>? teachers = null,
        IReadOnlyList<int>? rooms = null,
        IReadOnlyList<int>? substitutedTeacherIds = null,
        IReadOnlyList<int>? subjects = null,
        DateOnly? date = null,
        TimeOnly? start = null,
        TimeOnly? end = null)
        => new(
            Id: id,
            Date: date ?? new DateOnly(2026, 9, 28),
            Start: start ?? new TimeOnly(8, 50),
            End: end ?? new TimeOnly(9, 40),
            Status: status,
            SubstitutedTeacherIds: substitutedTeacherIds ?? [],
            Classes: [],
            Teachers: teachers ?? [],
            Subjects: subjects ?? [],
            Rooms: rooms ?? []);

}
