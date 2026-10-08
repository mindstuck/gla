using GLA.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace GLA.Tests;

/// <summary>
/// Creates a <see cref="GlaDbContext"/> backed by a real SQLite database held in memory,
/// so relational behaviour (constraints, types) is exercised against a real engine —
/// the same provider the application runs on.
/// The connection must stay open for the lifetime of the test, hence IDisposable.
/// </summary>
public sealed class SqliteGlaDbContext : IDisposable
{
    private readonly SqliteConnection _connection;

    public SqliteGlaDbContext()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<GlaDbContext>()
            .UseSqlite(_connection)
            .Options;

        Context = new GlaDbContext(options);
        Context.Database.EnsureCreated();
    }

    public GlaDbContext Context { get; }

    /// <summary>
    /// Removes the HasData seed songs so tests can start from an empty database.
    /// </summary>
    public void ClearSongs()
    {
        Context.Songs.RemoveRange(Context.Songs);
        Context.SaveChanges();
        Context.ChangeTracker.Clear();
    }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
    }
}
