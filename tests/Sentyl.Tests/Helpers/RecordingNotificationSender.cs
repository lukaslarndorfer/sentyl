using Sentyl.Notifications;

namespace Sentyl.Tests.Helpers;

internal sealed class RecordingNotificationSender : INotificationSender
{
    internal List<LessonChangeGroup> SentGroups { get; } = [];

    public Task SendAsync(LessonChangeGroup group, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        SentGroups.Add(group);
        return Task.CompletedTask;
    }
}