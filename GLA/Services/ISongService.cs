using GLA.Dtos;

namespace GLA.Services;

public interface ISongService
{
    Task<SongDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SongDto>> ListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores an uploaded score file under the files root and creates the
    /// matching song row. Throws <see cref="UnsupportedScoreFileException"/>
    /// when the extension is not an alphaTab score format.
    /// </summary>
    Task<SongDto> CreateAsync(Stream content, string fileName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the song and the score file that belongs to it.
    /// False when the id does not exist.
    /// </summary>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
