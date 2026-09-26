using GLA.Dtos;
using GLA.Repositories;

namespace GLA.Services;

public class SongService(ISongRepository songRepository) : ISongService
{
    public async Task<SongDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var song = await songRepository.GetByIdAsync(id, cancellationToken);
        return song is null ? null : SongDto.From(song);
    }

    public async Task<IReadOnlyList<SongDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var songs = await songRepository.ListAsync(cancellationToken);
        return songs.Select(SongDto.From).ToList();
    }
}
