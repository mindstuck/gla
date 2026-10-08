namespace GLA.Services;

/// <summary>
/// Resolves a song's <c>FilePath</c> (as stored in the database) to a physical
/// file under the configured score-files root (<c>Songs:FilesRoot</c>).
/// Paths are stored as plain relative paths without a leading separator
/// (e.g. <c>BlackHoleSun.gp4</c>, subfolders use <c>/</c>).
/// Returns <c>null</c> when the path is empty, escapes the root, or the file
/// does not exist — the caller maps all three cases to 404.
/// </summary>
public class ScoreFileLocator(string filesRoot)
{
    private readonly string _filesRoot =
        Path.TrimEndingDirectorySeparator(Path.GetFullPath(filesRoot));

    private static StringComparison PathComparison =>
        OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

    public string? Locate(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return null;
        }

        string resolved;
        try
        {
            resolved = Path.GetFullPath(Path.Combine(_filesRoot, filePath));
        }
        catch (Exception e) when (e is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return null;
        }

        // Defence in depth: rooted or "../" paths must not escape the files
        // root — a bad value in the database fails safe with 404.
        if (!resolved.StartsWith(_filesRoot + Path.DirectorySeparatorChar, PathComparison))
        {
            return null;
        }

        return File.Exists(resolved) ? resolved : null;
    }
}
