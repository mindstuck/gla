using GLA.Entities;
using GLA.Repositories;

namespace GLA.Tests;

/// <summary>
/// In-memory <see cref="ISongRepository"/> for unit tests — no database involved.
/// </summary>
public class FakeSongRepository : ISongRepository
{
    private readonly List<SongEntity> _songs = new();

    public FakeSongRepository(params SongEntity[] songs)
    {
        _songs.AddRange(songs);
    }

    public Task<SongEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var song = _songs.FirstOrDefault(s => s.Id == id);
        return Task.FromResult(song);
    }

    public Task<IReadOnlyList<SongEntity>> ListAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<SongEntity> result = _songs.OrderBy(s => s.Id).ToList();
        return Task.FromResult(result);
    }

    public Task<SongEntity> AddAsync(SongEntity song, CancellationToken cancellationToken = default)
    {
        // The real repository gets its id from the database; the fake assigns
        // the next free one so tests can address the new row.
        song.Id = _songs.Count == 0 ? 1 : _songs.Max(s => s.Id) + 1;
        _songs.Add(song);
        return Task.FromResult(song);
    }

    public Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_songs.RemoveAll(s => s.Id == id) > 0);
    }
}
