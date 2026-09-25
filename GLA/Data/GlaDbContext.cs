using GLA.Entities;
using Microsoft.EntityFrameworkCore;

namespace GLA.Data;

public class GlaDbContext(DbContextOptions<GlaDbContext> options) : DbContext(options)
{
    public DbSet<SongEntity> Songs => Set<SongEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var song = modelBuilder.Entity<SongEntity>();

        song.ToTable("Songs");
        song.HasKey(s => s.Id);
        song.Property(s => s.Title).HasMaxLength(200).IsRequired();
        song.Property(s => s.Author).HasMaxLength(200).IsRequired();
        song.Property(s => s.FilePath).HasMaxLength(500).IsRequired();

        var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        song.HasData(
            new SongEntity
            {
                Id = 1,
                Title = "Stairway to Heaven",
                Author = "Led Zeppelin",
                FilePath = "led-zeppelin-stairway_to_heaven.gp4",
                CreatedAt = seedDate,
                UpdatedAt = seedDate,
            },
            new SongEntity
            {
                Id = 2,
                Title = "Paranoid Android",
                Author = "Radiohead",
                FilePath = "ParanoidAndroid.gp5",
                CreatedAt = seedDate,
                UpdatedAt = seedDate,
            },
            new SongEntity
            {
                Id = 3,
                Title = "Black Hole Sun",
                Author = "Soundgarden",
                FilePath = "BlackHoleSun.gp4",
                CreatedAt = seedDate,
                UpdatedAt = seedDate,
            },
            new SongEntity
            {
                Id = 4,
                Title = "Nothing Else Matters",
                Author = "Metallica",
                FilePath = "NothingElseMatters.gp5",
                CreatedAt = seedDate,
                UpdatedAt = seedDate,
            });
    }
}
