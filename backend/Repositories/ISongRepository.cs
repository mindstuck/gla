using GLA.Entities;

namespace GLA.Repositories;

public interface ISongRepository
{
    Task<SongEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SongEntity>> ListAsync(CancellationToken cancellationToken = default);

    /// <summary>Inserts the song and fills in its generated Id.</summary>
    Task<SongEntity> AddAsync(SongEntity song, CancellationToken cancellationToken = default);

    /// <summary>Deletes the row; false when the id does not exist.</summary>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
