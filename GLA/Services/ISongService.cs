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
}
