using GLA.Dtos;
using GLA.Entities;
using GLA.Repositories;

namespace GLA.Services;

public class SongService(
    ISongRepository songRepository,
    SongFileStore fileStore,
    ScoreFileLocator fileLocator) : ISongService
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

    public async Task<SongDto> CreateAsync(
        Stream content,
        string fileName,
        string? title = null,
        string? author = null,
        CancellationToken cancellationToken = default)
    {
        if (!fileStore.IsSupported(fileName))
        {
            throw new UnsupportedScoreFileException(
                $"Unsupported score format. Supported: {string.Join(", ", SongFileStore.SupportedExtensions)}");
        }

        var filePath = await fileStore.SaveAsync(content, fileName, cancellationToken);

        // The client may pass title/author with the upload; when it doesn't,
        // the file name is all we have until the score is loaded with alphaTab.
        var entity = new SongEntity
        {
            Title = TitleFrom(title, fileName),
            Author = AuthorFrom(author),
            FilePath = filePath,
        };

        await songRepository.AddAsync(entity, cancellationToken);
        return SongDto.From(entity);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var song = await songRepository.GetByIdAsync(id, cancellationToken);
        if (song is null)
        {
            return false;
        }

        var deleted = await songRepository.DeleteAsync(id, cancellationToken);
        if (deleted)
        {
            // Row first: if file removal fails (locked, already gone) only an
            // orphaned file remains, never a row without its score.
            // Locate() re-checks the path against the files root.
            var path = fileLocator.Locate(song.FilePath);
            if (path is not null)
            {
                try
                {
                    File.Delete(path);
                }
                catch (Exception e) when (e is IOException or UnauthorizedAccessException)
                {
                    // Best effort — the song is already gone.
                }
            }
        }

        return deleted;
    }

    /// <summary>
    /// An explicitly provided title wins (trimmed, capped to the column);
    /// otherwise the uploaded file's name without extension.
    /// </summary>
    private static string TitleFrom(string? title, string fileName)
    {
        var provided = title?.Trim();
        if (provided is { Length: > 0 })
        {
            return provided.Length > 200 ? provided[..200] : provided;
        }

        var name = Path.GetFileName(fileName ?? string.Empty);
        var derived = Path.GetFileNameWithoutExtension(name).Trim();
        if (derived.Length == 0)
        {
            derived = "Untitled";
        }

        return derived.Length > 200 ? derived[..200] : derived;
    }

    /// <summary>The optional author, trimmed and capped; empty when absent.</summary>
    private static string AuthorFrom(string? author)
    {
        var value = author?.Trim() ?? string.Empty;
        return value.Length > 200 ? value[..200] : value;
    }
}
