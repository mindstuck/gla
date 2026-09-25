using GLA.Dtos;

namespace GLA.Services;

public interface ISongService
{
    Task<SongDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SongDto>> ListAsync(CancellationToken cancellationToken = default);
}
