using System;

using Sentyl.Timetable;

namespace Sentyl.Untis;

public static class LessonMapper
{

    public static Lesson ToLesson(this TimetableEntry entry)
    {
        LessonStatus status = entry.Code switch
        {
            null => LessonStatus.Regular,
            "cancelled" => LessonStatus.Cancelled,
            "irregular" => LessonStatus.Irregular,
            _ => LessonStatus.Unknown
        };
        return new(entry.Id,
           UntisDateTimeExtensions.FromUntisDate(entry.Date),
           UntisDateTimeExtensions.FromUntisTime(entry.StartTime),
           UntisDateTimeExtensions.FromUntisTime(entry.EndTime),
           status,
           [.. entry.Teachers.Where(t => t.OrgId != null).Select(t => t.OrgId!.Value)],
           [.. entry.Classes.Select(c => c.Id)],
           [.. entry.Teachers.Select(t => t.Id)],
           [.. entry.Subjects.Select(s => s.Id)],
           [.. entry.Rooms.Select(r => r.Id)]
           );
    }


}
