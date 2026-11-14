using System.Data.Common;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using Sentyl.Persistence;

namespace Sentyl.Tests.Helpers;

public sealed class SqliteTestDatabase : IDisposable, IDbContextFactory<DatabaseContext>
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    public SqliteTestDatabase()
    {
        _connection.Open();

        using var db = CreateDbContext();
        db.Database.EnsureCreated();
    }

    public DatabaseContext CreateDbContext()
    {
        var builder = new DbContextOptionsBuilder<DatabaseContext>();
        PersistenceSetup.ConfigureDatabaseContextOptions(builder, _connection, sensitiveDataLogging: true);
        return new DatabaseContext(builder.Options);
    }

    public void Dispose() => _connection.Dispose();
}
