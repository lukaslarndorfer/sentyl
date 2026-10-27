namespace Sentyl.ChangeDetection;

public enum LessonChangeType
{
    Cancelled,
    CancellationWithdrawn,
    TeacherSubstituted,
    TeacherSubstitutionWithdrawn,
    TeacherSubstitutionChanged,
    RoomChanged, // room changed back to original room = also room changed
    LessonAdded,
    BecameIrregular
}
