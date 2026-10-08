namespace GLA.Services;

/// <summary>
/// Writes uploaded score files into the configured score-files root
/// (<c>Songs:FilesRoot</c>) — the write side of <see cref="ScoreFileLocator"/>,
/// which resolves stored paths back to disk when serving them.
///
/// A stored file name is never taken verbatim from the upload: any client-side
/// directory is stripped, the stem is reduced to characters that are safe on
/// every file system, and a "(2)"-style suffix is added when the name is
/// already taken — one song's upload can never overwrite another song's file.
/// </summary>
public class SongFileStore(string filesRoot)
{
    /// <summary>
    /// Score formats alphaTab can load. Single source of truth on the server;
    /// the client mirrors the same list in its file picker's accept attribute.
    /// </summary>
    public static IReadOnlyList<string> SupportedExtensions => Extensions;

    private static readonly string[] Extensions =
    [
        ".gp3", ".gp4", ".gp5", ".gpx", ".gp",
        ".musicxml", ".xml",
        ".capx", ".capalphaTex", ".alphatex",
    ];

    private static readonly HashSet<string> ExtensionSet =
        new(Extensions, StringComparer.OrdinalIgnoreCase);

    private readonly string _filesRoot =
        Path.TrimEndingDirectorySeparator(Path.GetFullPath(filesRoot));

    /// <summary>True when the file name ends in a supported score format.</summary>
    public bool IsSupported(string? fileName) =>
        !string.IsNullOrWhiteSpace(fileName) && ExtensionSet.Contains(Path.GetExtension(fileName));

    /// <summary>
    /// Stores the upload and returns its path relative to the files root
    /// (forward slashes — the same shape as <c>SongEntity.FilePath</c>).
    /// </summary>
    public async Task<string> SaveAsync(Stream content, string fileName, CancellationToken cancellationToken)
    {
        var storedName = BuildStoredName(fileName);
        var physicalPath = Path.Combine(_filesRoot, storedName);

        try
        {
            await using var output = File.Create(physicalPath);
            await content.CopyToAsync(output, cancellationToken);
        }
        catch
        {
            // A failed or cancelled copy must not leave a truncated score behind.
            File.Delete(physicalPath);
            throw;
        }

        return storedName.Replace(Path.DirectorySeparatorChar, '/');
    }

    /// <summary>
    /// Sanitised stem plus the lower-cased extension, suffixed "(2)", "(3)", …
    /// while that name is taken.
    /// </summary>
    private string BuildStoredName(string fileName)
    {
        // FileName may arrive as a client-side path ("C:\fake\song.gp5"); only
        // the name itself is ever used, and GetFullPath keeps the final
        // physical path inside the files root by construction.
        var name = Path.GetFileName(fileName ?? string.Empty);
        var extension = Path.GetExtension(name).ToLowerInvariant();
        var stem = Sanitize(Path.GetFileNameWithoutExtension(name));
        if (stem.Length == 0)
        {
            stem = "song";
        }

        var candidate = stem + extension;
        for (var suffix = 2; File.Exists(Path.Combine(_filesRoot, candidate)); suffix++)
        {
            candidate = $"{stem} ({suffix}){extension}";
        }

        return candidate;
    }

    private static string Sanitize(string stem)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var chars = stem.Select(c => invalid.Contains(c) || char.IsControl(c) ? '_' : c).ToArray();

        // No trailing dots (Windows trims them) or leading dots (hidden files).
        var cleaned = new string(chars).Trim().Trim('.');

        return cleaned.Length > 150 ? cleaned[..150].Trim() : cleaned;
    }
}
