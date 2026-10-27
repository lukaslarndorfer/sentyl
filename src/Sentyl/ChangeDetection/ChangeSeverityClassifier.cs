namespace Sentyl.ChangeDetection;

public static class ChangeSeverityClassifier
{
    public static NotificationSeverity Classify(LessonChange change, TimeProvider timeProvider)
    {
        DateTime now = timeProvider.GetLocalNow().DateTime;
        DateTime start = change.Current.Date.ToDateTime(change.Current.Start);
        DateTime end = change.Current.Date.ToDateTime(change.Current.End);
        if (now < end && start < now.AddDays(1) && IsPingType(change.Type))
        {
            return NotificationSeverity.Ping;
        }
        return NotificationSeverity.Info;
    }

    private static bool IsPingType(LessonChangeType changeType)
    {
        return changeType switch
        {
            LessonChangeType.Cancelled
            or LessonChangeType.CancellationWithdrawn
            or LessonChangeType.TeacherSubstituted
            or LessonChangeType.TeacherSubstitutionWithdrawn
            or LessonChangeType.RoomChanged
            or LessonChangeType.LessonAdded
            or LessonChangeType.BecameIrregular => true,
            LessonChangeType.TeacherSubstitutionChanged => false,
            _ => throw new ArgumentOutOfRangeException(nameof(changeType), changeType, "Enum Value is not classified"),
        };
    }

}
