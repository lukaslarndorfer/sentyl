using System;

using Sentyl.Timetable;

namespace Sentyl.ChangeDetection;

public static class LessonChangeDetector
{
    public static IReadOnlyList<LessonChange> DetectChanges(IReadOnlyList<Lesson> previous, IReadOnlyList<Lesson> current)
    {
        if (previous.Count == 0)
        {
            return []; // no previous lessons, so we don't consider this a change (can be initial fetch)
        }

        List<LessonChange> changes = [];
        var previousLessonsById = previous.ToDictionary(l => l.Id);

        var earliestDate = previous.Min(l => l.Date);
        var latestDate = previous.Max(l => l.Date);
        foreach (Lesson lesson in current)
        {
            if (!previousLessonsById.TryGetValue(lesson.Id, out var prevLesson))
            {
                if (lesson.Date <= latestDate && lesson.Date >= earliestDate)
                {
                    changes.Add(new LessonChange(LessonChangeType.LessonAdded, null, lesson));
                }
            }
            else
            {
                changes.AddRange(DetectLessonChanges(prevLesson, lesson));
            }
        }

        return changes;
    }

    private static IEnumerable<LessonChange> DetectLessonChanges(Lesson previous, Lesson current)
    {
        if (previous.Status != current.Status)
        {
            if (current.Status == LessonStatus.Cancelled)
            {
                yield return new LessonChange(LessonChangeType.Cancelled, previous, current);
            }
            if (previous.Status == LessonStatus.Cancelled && current.Status == LessonStatus.Regular)
            {
                yield return new LessonChange(LessonChangeType.CancellationWithdrawn, previous, current);
            }
            if (current.Status == LessonStatus.Irregular)
            {
                yield return new LessonChange(LessonChangeType.BecameIrregular, previous, current);
            }
        }
        if (current.SubstitutedTeacherIds.Count > 0 && previous.SubstitutedTeacherIds.Count == 0)
        {
            yield return new LessonChange(LessonChangeType.TeacherSubstituted, previous, current);
        }
        if (current.SubstitutedTeacherIds.Count > 0 && previous.SubstitutedTeacherIds.Count > 0
        && !current.Teachers.Order().SequenceEqual(previous.Teachers.Order()))
        {
            yield return new LessonChange(LessonChangeType.TeacherSubstitutionChanged, previous, current);
        }
        if (current.SubstitutedTeacherIds.Count == 0 && previous.SubstitutedTeacherIds.Count > 0)
        {
            yield return new LessonChange(LessonChangeType.TeacherSubstitutionWithdrawn, previous, current);
        }
        if (!current.Rooms.Order().SequenceEqual(previous.Rooms.Order()))
        {
            yield return new LessonChange(LessonChangeType.RoomChanged, previous, current);
        }

    }
}
