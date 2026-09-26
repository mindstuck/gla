using GLA.Entities;
using GLA.Services;

namespace GLA.Tests;

public class SongServiceTests
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

    private static SongService CreateService(params SongEntity[] songs) =>
        new(new FakeSongRepository(songs));

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
}
