using GLA.Entities;

namespace GLA.Dtos;

/// <summary>
/// Public representation of a song. Timestamps are intentionally not exposed.
/// </summary>
public record SongDto(int Id, string Title, string Author, string FilePath)
{
    public static SongDto From(SongEntity entity) =>
        new(entity.Id, entity.Title, entity.Author, entity.FilePath);
}
