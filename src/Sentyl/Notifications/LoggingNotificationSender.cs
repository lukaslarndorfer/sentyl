namespace Sentyl.Notifications;

public class LoggingNotificationSender(
    ILogger<LoggingNotificationSender> logger) : INotificationSender
{
    public Task SendAsync(LessonChangeGroup group, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Lesson changes detected for {Date} at {Start}: {ChangeCount} changes ({ChangeTypes})",
            group.Date,
            group.Start,
            group.Changes.Count,
            string.Join(", ", group.Changes.Select(change => change.Type)));

        return Task.CompletedTask;
    }
}
