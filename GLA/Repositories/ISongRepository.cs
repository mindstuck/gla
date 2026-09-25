using GLA.Entities;

namespace GLA.Repositories;

public interface ISongRepository
{
    Task<SongEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SongEntity>> ListAsync(CancellationToken cancellationToken = default);
}
