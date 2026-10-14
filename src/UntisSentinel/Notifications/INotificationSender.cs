namespace UntisSentinel.Notifications;

public interface INotificationSender
{
    public Task SendAsync(LessonChangeGroup group, CancellationToken cancellationToken);
}
