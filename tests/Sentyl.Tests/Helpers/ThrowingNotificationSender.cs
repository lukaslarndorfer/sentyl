using Sentyl.Notifications;

namespace Sentyl.Tests.Helpers;

internal sealed class ThrowingNotificationSender : INotificationSender
{
    public Task SendAsync(LessonChangeGroup group, CancellationToken cancellationToken)
    {
        throw new InvalidOperationException("Notification sending failed");
    }
}
