using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Sentyl.Persistence;

public sealed class DatabaseContext(DbContextOptions<DatabaseContext> options) : DbContext(options)
{
    public DbSet<GuildConfig> GuildConfigs { get; set; }
    public DbSet<GuildTimetableState> GuildTimetableStates { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureGuildConfig(modelBuilder);
        ConfigureGuildTimetableState(modelBuilder);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        configurationBuilder.Conventions.Remove<TableNameFromDbSetConvention>();

        // sqlite has no ulong
        configurationBuilder.Properties<ulong>().HaveConversion<long>();
    }

    private static void ConfigureGuildConfig(ModelBuilder modelBuilder)
    {
        EntityTypeBuilder<GuildConfig> guildConfig = modelBuilder.Entity<GuildConfig>();
        guildConfig.HasKey(g => g.GuildId);
        guildConfig.Property(g => g.GuildId).ValueGeneratedNever();
    }

    private static void ConfigureGuildTimetableState(ModelBuilder modelBuilder)
    {
        EntityTypeBuilder<GuildTimetableState> state = modelBuilder.Entity<GuildTimetableState>();
        state.HasKey(s => s.GuildId);
        state.Property(s => s.GuildId).ValueGeneratedNever();

        state.HasOne<GuildConfig>()
        .WithOne()
        .HasForeignKey<GuildTimetableState>(s => s.GuildId)
        .OnDelete(DeleteBehavior.Cascade);

        state.ComplexCollection(s => s.Lessons).ToJson();
    }
}
