using System;
using System.Collections.Frozen;

namespace Sentyl.Untis;

public class MasterData
{
    private readonly IReadOnlyDictionary<int, SchoolClass> _classes;
    private readonly IReadOnlyDictionary<int, Teacher> _teachers;
    private readonly IReadOnlyDictionary<int, Subject> _subjects;
    private readonly IReadOnlyDictionary<int, Room> _rooms;

    public MasterData(IEnumerable<SchoolClass> classes, IEnumerable<Teacher> teachers, IEnumerable<Subject> subjects, IEnumerable<Room> rooms)
    {
        // built once at startup and only read afterwards, so frozen (immutable, fast lookups)
        _classes = classes.ToFrozenDictionary(c => c.Id);
        _teachers = teachers.ToFrozenDictionary(t => t.Id);
        _subjects = subjects.ToFrozenDictionary(s => s.Id);
        _rooms = rooms.ToFrozenDictionary(r => r.Id);
    }

    public SchoolClass? GetSchoolClassById(int id) => _classes.GetValueOrDefault(id);
    public Teacher? GetTeacherById(int id) => _teachers.GetValueOrDefault(id);
    public Subject? GetSubjectById(int id) => _subjects.GetValueOrDefault(id);
    public Room? GetRoomById(int id) => _rooms.GetValueOrDefault(id);



}
