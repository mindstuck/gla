using GLA.Dtos;
using GLA.Entities;
using GLA.Repositories;

namespace GLA.Services;

public class SongService(ISongRepository songRepository, SongFileStore fileStore) : ISongService
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

    public async Task<SongDto> CreateAsync(Stream content, string fileName, CancellationToken cancellationToken = default)
    {
        if (!fileStore.IsSupported(fileName))
        {
            throw new UnsupportedScoreFileException(
                $"Unsupported score format. Supported: {string.Join(", ", SongFileStore.SupportedExtensions)}");
        }

        var filePath = await fileStore.SaveAsync(content, fileName, cancellationToken);

        // No metadata parsing on the server: the file name is all we have
        // until the client loads the score with alphaTab. Author stays empty.
        var entity = new SongEntity
        {
            Title = TitleFrom(fileName),
            Author = string.Empty,
            FilePath = filePath,
        };

        await songRepository.AddAsync(entity, cancellationToken);
        return SongDto.From(entity);
    }

    /// <summary>The uploaded file's name without extension, capped to the column size.</summary>
    private static string TitleFrom(string fileName)
    {
        var name = Path.GetFileName(fileName ?? string.Empty);
        var title = Path.GetFileNameWithoutExtension(name).Trim();
        if (title.Length == 0)
        {
            title = "Untitled";
        }

        return title.Length > 200 ? title[..200] : title;
    }
}
