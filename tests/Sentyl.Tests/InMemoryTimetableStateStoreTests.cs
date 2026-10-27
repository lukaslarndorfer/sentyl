using AwesomeAssertions;

using Sentyl.Tests.Helpers;
using Sentyl.Timetable;

namespace Sentyl.Tests;


public class InMemoryTimetableStateStoreTests
{
    [Fact]
    public async Task LoadAsync_WhenNothingWasSaved_ReturnsNull()
    {
        var store = new InMemoryTimetableStateStore();

        var result = await store.LoadAsync(CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task SaveAsync_ThenLoadAsync_ReturnsSavedLessons()
    {
        var store = new InMemoryTimetableStateStore();
        await store.SaveAsync([TestLessons.Create(id: 432)], CancellationToken.None);

        var result = await store.LoadAsync(CancellationToken.None);

        result.Should().ContainSingle().Subject.Id.Should().Be(432);
    }

    [Fact]
    public async Task SaveAsync_ReplacesPreviousLessons()
    {
        var store = new InMemoryTimetableStateStore();
        await store.SaveAsync([TestLessons.Create(status: LessonStatus.Regular)], CancellationToken.None);


        await store.SaveAsync([TestLessons.Create(status: LessonStatus.Cancelled)], CancellationToken.None);

        var result = await store.LoadAsync(CancellationToken.None);

        result.Should().ContainSingle().Subject.Status.Should().Be(LessonStatus.Cancelled);
    }
}
