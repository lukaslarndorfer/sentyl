using AwesomeAssertions;

using Microsoft.EntityFrameworkCore;

using Sentyl.Persistence;
using Sentyl.Tests.Helpers;
using Sentyl.Timetable;

namespace Sentyl.Tests.Persistence;

public sealed class DatabaseTimetableStateStoreTests : IDisposable
{
    // above long.MaxValue, so the ulong -> long conversion is part of the test
    private const ulong GuildId = 18_000_000_000_000_000_000UL;
    private const ulong OtherGuildId = 18_000_000_000_000_000_001UL;

    private readonly SqliteTestDatabase _database = new();
    private readonly DatabaseTimetableStateStore _store;

    public DatabaseTimetableStateStoreTests()
    {
        _store = new DatabaseTimetableStateStore(_database);
    }

    public void Dispose() => _database.Dispose();

    [Fact]
    public async ValueTask LoadAsync_WhenNothingWasSaved_ReturnsNull()
    {
        IReadOnlyList<Lesson>? result = await _store.LoadAsync(GuildId, TestContext.Current.CancellationToken);
        result.Should().BeNull();
    }

    [Fact]
    public async ValueTask SaveAsync_ThenLoadAsync_ReturnsSavedLessons()
    {
        await AddGuildAsync(GuildId);
        await _store.SaveAsync(GuildId, [TestLessons.Create(321)], TestContext.Current.CancellationToken);

        IReadOnlyList<Lesson>? result = await _store.LoadAsync(GuildId, TestContext.Current.CancellationToken);
        result.Should().ContainSingle().Subject.Id.Should().Be(321);
    }

    [Fact]
    public async ValueTask SaveAsync_ReplacesPreviousLessons()
    {
        await AddGuildAsync(GuildId);
        await _store.SaveAsync(GuildId, [TestLessons.Create(321, status: LessonStatus.Regular)],
                               TestContext.Current.CancellationToken);
        await _store.SaveAsync(GuildId, [TestLessons.Create(321, status: LessonStatus.Cancelled)],
                               TestContext.Current.CancellationToken);

        IReadOnlyList<Lesson>? result = await _store.LoadAsync(GuildId, TestContext.Current.CancellationToken);

        result.Should().ContainSingle().Subject.Status.Should().Be(LessonStatus.Cancelled);
    }

    [Fact]
    public async ValueTask SaveAsync_WhenFirstSaveWasEmpty_UpdatesExistingState()
    {
        await AddGuildAsync(GuildId);
        await _store.SaveAsync(GuildId, [],
                               TestContext.Current.CancellationToken);
        await _store.SaveAsync(GuildId, [TestLessons.Create(321, status: LessonStatus.Regular)],
                               TestContext.Current.CancellationToken);

        IReadOnlyList<Lesson>? result = await _store.LoadAsync(GuildId, TestContext.Current.CancellationToken);

        result.Should().ContainSingle().Subject.Id.Should().Be(321);
    }

    [Fact]
    public async ValueTask LoadAsync_KeepsStatesOfDifferentGuildsSeparate()
    {
        await AddGuildAsync(GuildId);
        await AddGuildAsync(OtherGuildId);

        await _store.SaveAsync(GuildId, [TestLessons.Create(321, status: LessonStatus.Regular)],
                               TestContext.Current.CancellationToken);

        IReadOnlyList<Lesson>? result = await _store.LoadAsync(OtherGuildId, TestContext.Current.CancellationToken);

        result.Should().BeNull();
    }

    [Fact]
    public async ValueTask SaveAsync_WhenGuildHasNoConfig_Throws()
    {
        await Assert.ThrowsAsync<DbUpdateException>(() => _store.SaveAsync(GuildId, [TestLessons.Create()],
                                                                           TestContext.Current.CancellationToken));
    }

    private async Task AddGuildAsync(ulong guildId)
    {
        await using var db = _database.CreateDbContext();
        db.GuildConfigs.Add(new GuildConfig
        {
            GuildId = guildId,
            ChannelId = 1,
            UntisHost = "abc.untis.com",
            UntisSchool = "abc-school",
            UntisUser = "user",
            UntisPasswordEncrypted = "encrypted",
            Enabled = true
        });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
    }
}
