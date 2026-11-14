using AwesomeAssertions;
using Sentyl.Persistence;
using Sentyl.Tests.Helpers;

namespace Sentyl.Tests.Persistence;

public sealed class DatabaseGuildConfigStoreTests : IDisposable
{
    // above long.MaxValue, so the ulong -> long conversion is part of the test
    private const ulong GuildId = 18_000_000_000_000_000_000UL;
    private const ulong OtherGuildId = 18_000_000_000_000_000_001UL;

    private readonly SqliteTestDatabase _database = new();
    private readonly DatabaseGuildConfigStore _store;

    public DatabaseGuildConfigStoreTests()
    {
        _store = new DatabaseGuildConfigStore(_database);
    }

    public void Dispose() => _database.Dispose();

    [Fact]
    public async ValueTask GetAsync_WhenNothingWasSaved_ReturnsNull()
    {
        var result = await _store.GetAsync(GuildId, TestContext.Current.CancellationToken);

        result.Should().BeNull();
    }

    [Fact]
    public async ValueTask UpsertAsync_ThenGetAsync_ReturnsSavedConfig()
    {
        var config = CreateConfig(GuildId);
        await _store.UpsertAsync(config, TestContext.Current.CancellationToken);

        var result = await _store.GetAsync(GuildId, TestContext.Current.CancellationToken);

        result.Should().BeEquivalentTo(config);
    }

    [Fact]
    public async ValueTask UpsertAsync_WhenConfigExists_ReplacesValues()
    {
        await _store.UpsertAsync(CreateConfig(GuildId), TestContext.Current.CancellationToken);
        var changed = CreateConfig(GuildId, enabled: false);
        changed.ChannelId = 2;
        changed.UntisHost = "other.untis.com";
        await _store.UpsertAsync(changed, TestContext.Current.CancellationToken);

        var result = await _store.GetAsync(GuildId, TestContext.Current.CancellationToken);

        result.Should().BeEquivalentTo(changed);
    }

    [Fact]
    public async ValueTask GetEnabledAsync_ReturnsOnlyEnabledGuilds()
    {
        await _store.UpsertAsync(CreateConfig(GuildId, enabled: false), TestContext.Current.CancellationToken);
        await _store.UpsertAsync(CreateConfig(OtherGuildId, enabled: true), TestContext.Current.CancellationToken);

        IReadOnlyList<GuildConfig> result = await _store.GetEnabledAsync(TestContext.Current.CancellationToken);

        result.Should().ContainSingle().Subject.GuildId.Should().Be(OtherGuildId);
    }

    [Fact]
    public async ValueTask DeleteAsync_RemovesConfigAndState()
    {
        DatabaseTimetableStateStore stateStore = new(_database);
        await _store.UpsertAsync(CreateConfig(GuildId), TestContext.Current.CancellationToken);
        await stateStore.SaveAsync(GuildId, [TestLessons.Create()], TestContext.Current.CancellationToken);

        await _store.DeleteAsync(GuildId, TestContext.Current.CancellationToken);

        (await _store.GetAsync(GuildId, TestContext.Current.CancellationToken)).Should().BeNull();
        (await stateStore.LoadAsync(GuildId, TestContext.Current.CancellationToken)).Should().BeNull();
    }

    [Fact]
    public async ValueTask DeleteAsync_WhenGuildDoesNotExist_DoesNotAffectOtherGuilds()
    {
        await _store.UpsertAsync(CreateConfig(GuildId), TestContext.Current.CancellationToken);

        await _store.DeleteAsync(OtherGuildId, TestContext.Current.CancellationToken);

        var guild = await _store.GetAsync(GuildId, TestContext.Current.CancellationToken);
        guild.Should().NotBeNull();
    }

    private static GuildConfig CreateConfig(ulong guildId, bool enabled = true) =>
        new()
        {
            GuildId = guildId,
            ChannelId = 1_088_445_242_144_993_343UL,
            UntisHost = "abc.untis.com",
            UntisSchool = "abc-school",
            UntisUser = "user",
            UntisPasswordEncrypted = "encrypted",
            Enabled = enabled
        };
}
