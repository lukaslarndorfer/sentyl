using UntisSentinel.ChangeDetection;

namespace UntisSentinel.Notifications;

// parallel running lessons
public sealed record LessonChangeGroup(DateOnly Date, TimeOnly Start, IReadOnlyList<LessonChange> Changes);
