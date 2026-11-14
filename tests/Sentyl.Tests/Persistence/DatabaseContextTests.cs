using AwesomeAssertions;

using Microsoft.EntityFrameworkCore;

using Sentyl.Persistence;
using Sentyl.Tests.Helpers;
using Sentyl.Timetable;

namespace Sentyl.Tests.Persistence;

public sealed class DatabaseContextTests : IDisposable
{
    // above long.MaxValue, so the ulong -> long conversion is part of the test
    private const ulong GuildId = 18_000_000_000_000_000_000UL;

    private readonly SqliteTestDatabase _database = new();

    public void Dispose() => _database.Dispose();

    [Fact]
    public async ValueTask SaveAndLoad_RoundTripsConfigAndLessons()
    {
        var config = CreateConfig();
        Lesson[] lessons = [TestLessons.Create(id: 1, teachers: [1, 2], rooms: [3]), TestLessons.Create(id: 2)];

        await using (var db = CreateContext())
        {
            db.GuildConfigs.Add(config);
            db.GuildTimetableStates.Add(new GuildTimetableState { GuildId = GuildId, Lessons = [.. lessons] });
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using var verify = CreateContext();
        var loadedConfig = await verify.GuildConfigs.SingleAsync(TestContext.Current.CancellationToken);
        var loadedState = await verify.GuildTimetableStates.SingleAsync(TestContext.Current.CancellationToken);

        loadedConfig.Should().BeEquivalentTo(config);
        loadedState.Lessons.Should().BeEquivalentTo(lessons, options => options.WithStrictOrdering());
    }

    [Fact]
    public async ValueTask Update_ReplacesLessons()
    {
        await using (var db = CreateContext())
        {
            db.GuildConfigs.Add(CreateConfig());
            db.GuildTimetableStates.Add(new GuildTimetableState { GuildId = GuildId, Lessons = [TestLessons.Create(id: 1)] });
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using (var db = CreateContext())
        {
            var state = await db.GuildTimetableStates.SingleAsync(TestContext.Current.CancellationToken);
            state.Lessons = [TestLessons.Create(id: 2)];
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using var verify = CreateContext();
        var loaded = await verify.GuildTimetableStates.SingleAsync(TestContext.Current.CancellationToken);

        loaded.Lessons.Select(l => l.Id).Should().Equal(2);
    }

    [Fact]
    public async ValueTask DeleteConfig_CascadesToState()
    {
        await using (var db = CreateContext())
        {
            db.GuildConfigs.Add(CreateConfig());
            db.GuildTimetableStates.Add(new GuildTimetableState { GuildId = GuildId, Lessons = [TestLessons.Create()] });
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using (var db = CreateContext())
        {
            await db.GuildConfigs.ExecuteDeleteAsync(TestContext.Current.CancellationToken);
        }

        await using var verify = CreateContext();
        (await verify.GuildTimetableStates.AnyAsync(TestContext.Current.CancellationToken)).Should().BeFalse();
    }

    private DatabaseContext CreateContext() => _database.CreateDbContext();

    private static GuildConfig CreateConfig() => new()
    {
        GuildId = GuildId,
        ChannelId = 1_088_445_242_144_993_343UL,
        UntisHost = "abc.untis.com",
        UntisSchool = "abc-school",
        UntisUser = "user",
        UntisPasswordEncrypted = "encrypted",
        Enabled = true
    };
}
