using GLA.Services;

namespace GLA.Tests;

public class ScoreFileLocatorTests : IDisposable
{
    private readonly string _root;
    private readonly string _outside;
    private readonly ScoreFileLocator _locator;

    public ScoreFileLocatorTests()
    {
        var temp = Path.Combine(Path.GetTempPath(), "gla-locator-tests-" + Guid.NewGuid());
        _root = Path.Combine(temp, "storage");
        _outside = temp;
        Directory.CreateDirectory(_root);

        File.WriteAllText(Path.Combine(_root, "ExistingSong.gp4"), "score-bytes");
        File.WriteAllText(Path.Combine(_outside, "EscapedSong.gp4"), "score-bytes");

        _locator = new ScoreFileLocator(_root);
    }

    [Fact]
    public void Locate_ExistingFile_ReturnsFullPath()
    {
        var result = _locator.Locate("ExistingSong.gp4");

        Assert.Equal(Path.Combine(_root, "ExistingSong.gp4"), result);
    }

    [Fact]
    public void Locate_MissingFile_ReturnsNull()
    {
        Assert.Null(_locator.Locate("MissingSong.gp5"));
    }

    [Fact]
    public void Locate_PathEscapingRoot_ReturnsNullEvenWhenFileExists()
    {
        Assert.Null(_locator.Locate("../EscapedSong.gp4"));
    }

    [Fact]
    public void Locate_RootedPath_ReturnsNull()
    {
        Assert.Null(_locator.Locate("/ExistingSong.gp4"));
    }

    [Fact]
    public void Locate_EmptyOrNullPath_ReturnsNull()
    {
        Assert.Null(_locator.Locate(""));
        Assert.Null(_locator.Locate("   "));
        Assert.Null(_locator.Locate(null));
        Assert.Null(_locator.Locate("/"));
    }

    [Fact]
    public void Locate_PathInSubfolder_ResolvesInsideRoot()
    {
        var subfolder = Path.Combine(_root, "user123");
        Directory.CreateDirectory(subfolder);
        File.WriteAllText(Path.Combine(subfolder, "NestedSong.gp4"), "score-bytes");

        var result = _locator.Locate("user123/NestedSong.gp4");

        Assert.Equal(Path.Combine(subfolder, "NestedSong.gp4"), result);
    }

    [Fact]
    public void FromPath_GuitarProAndCapella_UseOctetStream()
    {
        Assert.Equal(ScoreFileContentTypes.Default, ScoreFileContentTypes.FromPath("song.gp"));
        Assert.Equal(ScoreFileContentTypes.Default, ScoreFileContentTypes.FromPath("song.gp3"));
        Assert.Equal(ScoreFileContentTypes.Default, ScoreFileContentTypes.FromPath("song.gp4"));
        Assert.Equal(ScoreFileContentTypes.Default, ScoreFileContentTypes.FromPath("song.gp5"));
        Assert.Equal(ScoreFileContentTypes.Default, ScoreFileContentTypes.FromPath("song.gpx"));
        Assert.Equal(ScoreFileContentTypes.Default, ScoreFileContentTypes.FromPath("song.cap"));
    }

    [Fact]
    public void FromPath_MusicXmlAndMidi_UseSpecificTypes()
    {
        Assert.Equal("application/vnd.recordare.musicxml+xml", ScoreFileContentTypes.FromPath("song.musicxml"));
        Assert.Equal("application/vnd.recordare.musicxml-zip", ScoreFileContentTypes.FromPath("song.mxl"));
        Assert.Equal("audio/midi", ScoreFileContentTypes.FromPath("song.mid"));
        Assert.Equal("audio/midi", ScoreFileContentTypes.FromPath("song.MIDI"));
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(Path.Combine(_root, ".."), recursive: true);
        }
        catch (IOException)
        {
            // Best-effort cleanup of the temp directory.
        }
    }
}
