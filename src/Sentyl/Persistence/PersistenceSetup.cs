using System.Data.Common;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;

namespace Sentyl.Persistence;

public static class PersistenceSetup
{
    extension(IServiceCollection services)
    {
        public void ConfigurePersistence(bool isDev)
        {
            services.AddDbContextFactory<DatabaseContext>((serviceProvider, optionsBuilder) =>
            {
                var storage = serviceProvider.GetRequiredService<IOptions<StorageOptions>>().Value;
                var connectionString = new SqliteConnectionStringBuilder
                {
                    DataSource = storage.DatabasePath
                }.ToString();

                ConfigureDatabaseContextOptions(optionsBuilder, connectionString, isDev);
            });
        }
    }

    extension(IHost host)
    {
        public async Task MigrateDatabaseAsync(CancellationToken cancellationToken = default)
        {
            IDbContextFactory<DatabaseContext> factory = host.Services.GetRequiredService<IDbContextFactory<DatabaseContext>>();
            await using var db = await factory.CreateDbContextAsync(cancellationToken);

            await db.Database.MigrateAsync(cancellationToken);
        }
    }

    public static void ConfigureDatabaseContextOptions(DbContextOptionsBuilder optionsBuilder, string connectionString,
                                                       bool sensitiveDataLogging)
        => ApplyCommonOptions(optionsBuilder.UseSqlite(connectionString), sensitiveDataLogging);

    public static void ConfigureDatabaseContextOptions(DbContextOptionsBuilder optionsBuilder, DbConnection connection,
                                                       bool sensitiveDataLogging)
        => ApplyCommonOptions(optionsBuilder.UseSqlite(connection), sensitiveDataLogging);

    private static void ApplyCommonOptions(DbContextOptionsBuilder optionsBuilder, bool sensitiveDataLogging)
    {
        optionsBuilder.ConfigureWarnings(warnings =>
                                             warnings.Throw(RelationalEventId.MultipleCollectionIncludeWarning));

        if (sensitiveDataLogging)
        {
            optionsBuilder.EnableSensitiveDataLogging()
                          .EnableDetailedErrors();
        }
    }
}
