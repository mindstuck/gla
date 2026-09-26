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
}
