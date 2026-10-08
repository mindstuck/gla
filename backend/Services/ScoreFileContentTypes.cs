namespace GLA.Services;

/// <summary>
/// Content types for score file formats supported by alphaTab
/// (Guitar Pro 3-8, MusicXML, Capella, MIDI). alphaTab only needs the bytes,
/// but the correct type keeps intermediaries and dev tools honest.
/// </summary>
public static class ScoreFileContentTypes
{
    public const string Default = "application/octet-stream";

    public static string FromPath(string path) => Path.GetExtension(path).ToLowerInvariant() switch
    {
        ".musicxml" => "application/vnd.recordare.musicxml+xml",
        ".mxl" => "application/vnd.recordare.musicxml-zip",
        ".mid" or ".midi" => "audio/midi",
        _ => Default, // .gp, .gp3, .gp4, .gp5, .gpx, .cap
    };
}
