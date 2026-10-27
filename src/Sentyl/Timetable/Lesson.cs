using System;

namespace Sentyl.Timetable;

public sealed record Lesson(int Id, DateOnly Date,
TimeOnly Start,
TimeOnly End,
LessonStatus Status,
IReadOnlyList<int> SubstitutedTeacherIds,
IReadOnlyList<int> Classes,
IReadOnlyList<int> Teachers,
IReadOnlyList<int> Subjects,
IReadOnlyList<int> Rooms);
