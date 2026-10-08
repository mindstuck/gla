namespace GLA.Services;

/// <summary>
/// Raised when an upload's extension is not one of the alphaTab score formats
/// in <see cref="SongFileStore.SupportedExtensions"/>. Endpoints map it to 400.
/// </summary>
public class UnsupportedScoreFileException(string message) : Exception(message);
