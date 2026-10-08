namespace GLA.Entities;

public class SongEntity
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public string Title { get; set; } = string.Empty;

    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// Location of the score file relative to the files root
    /// (e.g. "user123/BlackHoleSun.gp4", forward slashes, no leading slash),
    /// served later as https://gla.orbitalgarden.net/songs/files/user123/BlackHoleSun.gp4
    /// </summary>
    public string FilePath { get; set; } = string.Empty;
}
