using Sentyl.Timetable;

namespace Sentyl.ChangeDetection;

public sealed record LessonChange(LessonChangeType Type, Lesson? Previous, Lesson Current);
