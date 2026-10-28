using System.Globalization;
using System.Text;

using Sentyl.ChangeDetection;
using Sentyl.Timetable;
using Sentyl.Untis;

namespace Sentyl.Notifications.Discord;

public class DiscordMessageFormatter(MasterData masterData)
{
    private static readonly CultureInfo FormatProvider = new("de-AT");

    public string Format(LessonChangeGroup group)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"**{group.Date.ToString("ddd dd.MM.", FormatProvider)} – {group.Start.ToString("HH:mm", FormatProvider)}**");
        foreach (var change in group.Changes)
        {
            switch (change.Type)
            {
                case LessonChangeType.Cancelled:
                    {
                        sb.AppendLine($"{SubjectNames(change.Current)} ({TeacherNames(change.Current)}) entfällt!");
                        break;
                    }
                case LessonChangeType.CancellationWithdrawn:
                    {
                        sb.AppendLine($"{SubjectNames(change.Current)} ({TeacherNames(change.Current)}) findet doch statt!");
                        break;
                    }
                case LessonChangeType.TeacherSubstituted:
                    {
                        sb.AppendLine($"{SubjectNames(change.Current)}: Vertretung durch {TeacherNames(change.Current)}");
                        break;
                    }
                case LessonChangeType.TeacherSubstitutionWithdrawn:
                    {
                        sb.AppendLine($"{SubjectNames(change.Current)}: Vertretung widerrufen! Aktuell: {TeacherNames(change.Current)}");
                        break;
                    }
                case LessonChangeType.TeacherSubstitutionChanged:
                    {
                        var subjects = SubjectNames(change.Current);
                        var previousTeachers = TeacherNames(RequirePrevious(change));
                        var currentTeachers = TeacherNames(change.Current);

                        sb.AppendLine($"{subjects}: Vertretung geändert! Aktuell: {currentTeachers}; Davor: {previousTeachers}");
                        break;
                    }
                case LessonChangeType.RoomChanged:
                    {
                        var previousRooms = RoomNames(RequirePrevious(change));
                        var currentRooms = RoomNames(change.Current);

                        sb.AppendLine($"{SubjectNames(change.Current)}: Raumwechsel: Von {previousRooms} zu {currentRooms}");
                        break;
                    }
                case LessonChangeType.LessonAdded:
                    {
                        sb.AppendLine($"{SubjectNames(change.Current)}: Stunde hinzugefügt!");
                        break;
                    }
                case LessonChangeType.BecameIrregular:
                    {
                        sb.AppendLine($"{SubjectNames(change.Current)} findet außerplanmäßig statt!");
                        break;
                    }
                default:
                    {
                        throw new ArgumentOutOfRangeException(nameof(change), change.Type, null);
                    }
            }
        }
        return sb.ToString();
    }

    private string TeacherNames(Lesson lesson)
    {
        var teachers = lesson.Teachers.Select(t => masterData.GetTeacherById(t)?.Abbreviation ?? "?");
        return lesson.Teachers.Count == 0 ? "-" : string.Join(", ", teachers);
    }

    private string SubjectNames(Lesson lesson)
    {
        var subjects = lesson.Subjects.Select(s => masterData.GetSubjectById(s)?.Abbreviation ?? "?");
        return lesson.Subjects.Count == 0 ? "-" : string.Join(", ", subjects);
    }

    private string RoomNames(Lesson lesson)
    {
        var rooms = lesson.Rooms.Select(r => masterData.GetRoomById(r)?.Name ?? "?");
        return lesson.Rooms.Count == 0 ? "-" : string.Join(", ", rooms);
    }

    private Lesson RequirePrevious(LessonChange change)
    {
        return change.Previous ?? throw new InvalidOperationException($"LessonChange of type {change.Type} has no previous lesson");
    }
}
