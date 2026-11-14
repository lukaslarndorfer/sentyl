using Sentyl.Timetable;

namespace Sentyl.Persistence;

public class GuildTimetableState
{
    public ulong GuildId { get; set; }
    public List<Lesson> Lessons { get; set; } = [];
}
