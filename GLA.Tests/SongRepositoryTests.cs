using GLA.Entities;
using GLA.Repositories;

namespace GLA.Tests;

public class SongRepositoryTests : IDisposable
{
    private readonly SqliteGlaDbContext _db = new();
    private readonly SongRepository _repository;

    public SongRepositoryTests()
    {
        _repository = new SongRepository(_db.Context);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task GetById_ExistingSong_ReturnsSong()
    {
        var added = _db.Context.Songs.Add(
            new SongEntity { Title = "Fade to Black", Author = "Metallica", FilePath = "FadeToBlack.gp5" });
        await _db.Context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(added.Entity.Id);

        Assert.NotNull(result);
        Assert.Equal("Fade to Black", result.Title);
        Assert.Equal("Metallica", result.Author);
        Assert.Equal("FadeToBlack.gp5", result.FilePath);
    }

    [Fact]
    public async Task GetById_MissingSong_ReturnsNull()
    {
        var result = await _repository.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetById_ReturnsUntrackedEntity()
    {
        var added = _db.Context.Songs.Add(
            new SongEntity { Title = "Everlong", Author = "Foo Fighters", FilePath = "Everlong.gp5" });
        await _db.Context.SaveChangesAsync();
        _db.Context.ChangeTracker.Clear();

        var result = await _repository.GetByIdAsync(added.Entity.Id);

        Assert.NotNull(result);
        Assert.False(_db.Context.ChangeTracker.Entries<SongEntity>().Any());
    }

    [Fact]
    public async Task List_ReturnsAllSongsInIdOrder()
    {
        _db.ClearSongs();
        _db.Context.Songs.AddRange(
            new SongEntity { Title = "B", Author = "Author B", FilePath = "b.gp5" },
            new SongEntity { Title = "A", Author = "Author A", FilePath = "a.gp5" },
            new SongEntity { Title = "C", Author = "Author C", FilePath = "c.gp5" });
        await _db.Context.SaveChangesAsync();

        var result = await _repository.ListAsync();

        Assert.Equal(3, result.Count);
        Assert.Equal(
            result.Select(s => s.Id).OrderBy(id => id),
            result.Select(s => s.Id));
    }

    [Fact]
    public async Task List_EmptyDatabase_ReturnsEmptyList()
    {
        _db.ClearSongs();

        var result = await _repository.ListAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task SeededSongs_ArePresentAfterContextCreation()
    {
        var result = await _repository.ListAsync();

        Assert.Equal(4, result.Count);
        Assert.Contains(result, s => s.Id == 1 && s.Title == "Stairway to Heaven" && s.Author == "Led Zeppelin");
    }
}
