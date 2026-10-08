using GLA.Entities;
using GLA.Services;

namespace GLA.Tests;

public class SongServiceTests : IDisposable
{
    private static readonly SongEntity ParanoidAndroid = new()
    {
        Id = 1,
        Title = "Paranoid Android",
        Author = "Radiohead",
        FilePath = "ParanoidAndroid.gp5",
    };

    private static readonly SongEntity BlackHoleSun = new()
    {
        Id = 2,
        Title = "Black Hole Sun",
        Author = "Soundgarden",
        FilePath = "BlackHoleSun.gp4",
    };

    // The service also needs the file store and file locator (upload/delete);
    // point both at a per-test temp root so unit tests stay off the real
    // storage folder. Constructing SongFileStore creates the root.
    private readonly string _filesRoot =
        Path.Combine(Path.GetTempPath(), "gla-songservice-tests-" + Guid.NewGuid());

    private SongService CreateService(params SongEntity[] songs) =>
        new(new FakeSongRepository(songs), new SongFileStore(_filesRoot), new ScoreFileLocator(_filesRoot));

    [Fact]
    public async Task GetById_ExistingSong_ReturnsMappedDto()
    {
        var service = CreateService(ParanoidAndroid);

        var result = await service.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Paranoid Android", result.Title);
        Assert.Equal("Radiohead", result.Author);
        Assert.Equal("ParanoidAndroid.gp5", result.FilePath);
    }

    [Fact]
    public async Task GetById_MissingSong_ReturnsNull()
    {
        var service = CreateService(ParanoidAndroid);

        var result = await service.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task List_ReturnsAllSongsMapped()
    {
        var service = CreateService(ParanoidAndroid, BlackHoleSun);

        var result = await service.ListAsync();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, s => s.Title == "Paranoid Android" && s.Author == "Radiohead");
        Assert.Contains(result, s => s.Title == "Black Hole Sun" && s.Author == "Soundgarden");
    }

    [Fact]
    public async Task List_NoSongs_ReturnsEmptyList()
    {
        var service = CreateService();

        var result = await service.ListAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task Create_UnsupportedExtension_ThrowsWithoutStoring()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<UnsupportedScoreFileException>(
            () => service.CreateAsync(new MemoryStream([1, 2, 3]), "notes.txt"));

        Assert.Empty(await service.ListAsync());
    }

    [Fact]
    public async Task Create_SupportedFile_StoresRowAndBytes()
    {
        var service = CreateService();

        var result = await service.CreateAsync(new MemoryStream([1, 2, 3]), "Stairway to Heaven.gp4");

        // Title comes from the file name, author stays empty until the client
        // parses the score (no server-side metadata parsing).
        Assert.Equal("Stairway to Heaven", result.Title);
        Assert.Equal(string.Empty, result.Author);

        var stored = await service.GetByIdAsync(result.Id);
        Assert.NotNull(stored);
        Assert.True(File.Exists(Path.Combine(_filesRoot, result.FilePath)));
    }

    [Fact]
    public async Task Create_DuplicateFileName_GetsASuffixInsteadOfOverwriting()
    {
        var service = CreateService();

        var first = await service.CreateAsync(new MemoryStream([1]), "Same Song.gp4");
        var second = await service.CreateAsync(new MemoryStream([2]), "Same Song.gp4");

        // The store dedupes the stored file name; the title is just the stem.
        Assert.Equal("Same Song.gp4", first.FilePath);
        Assert.Equal("Same Song (2).gp4", second.FilePath);
        Assert.Equal("Same Song", first.Title);
        Assert.Equal("Same Song", second.Title);
        Assert.NotNull(await service.GetByIdAsync(first.Id));
        Assert.NotNull(await service.GetByIdAsync(second.Id));
        Assert.True(File.Exists(Path.Combine(_filesRoot, first.FilePath)));
        Assert.True(File.Exists(Path.Combine(_filesRoot, second.FilePath)));
    }

    [Fact]
    public async Task Create_ProvidedTitleAndAuthor_AreTrimmedAndUsed()
    {
        var service = CreateService();

        var result = await service.CreateAsync(
            new MemoryStream([1]), "Whatever.gp4", title: "  Bohemian Rhapsody  ", author: "  Queen  ");

        // The client's metadata wins over the file name.
        Assert.Equal("Bohemian Rhapsody", result.Title);
        Assert.Equal("Queen", result.Author);
        Assert.NotNull(await service.GetByIdAsync(result.Id));
    }

    [Fact]
    public async Task Create_BlankTitle_FallsBackToTheFileName()
    {
        var service = CreateService();

        var result = await service.CreateAsync(
            new MemoryStream([1]), "Kashmir.gp4", title: "   ", author: null);

        Assert.Equal("Kashmir", result.Title);
        Assert.Equal(string.Empty, result.Author);
    }

    [Fact]
    public async Task Delete_ExistingSong_RemovesRowThenFile()
    {
        var service = CreateService(ParanoidAndroid);
        var physicalPath = Path.Combine(_filesRoot, "ParanoidAndroid.gp5");
        await File.WriteAllTextAsync(physicalPath, "tab");

        var deleted = await service.DeleteAsync(1);

        Assert.True(deleted);
        Assert.Null(await service.GetByIdAsync(1));
        Assert.False(File.Exists(physicalPath));
    }

    [Fact]
    public async Task Delete_MissingSong_ReturnsFalseAndKeepsTheOthers()
    {
        var service = CreateService(ParanoidAndroid);

        var deleted = await service.DeleteAsync(999);

        Assert.False(deleted);
        Assert.NotNull(await service.GetByIdAsync(1));
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(_filesRoot, recursive: true);
        }
        catch (DirectoryNotFoundException)
        {
            // Nothing was stored — the root may never have been needed.
        }
    }
}
