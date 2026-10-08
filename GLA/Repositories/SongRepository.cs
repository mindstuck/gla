using GLA.Data;
using GLA.Entities;
using Microsoft.EntityFrameworkCore;

namespace GLA.Repositories;

public class SongRepository(GlaDbContext dbContext) : ISongRepository
{
    public Task<SongEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return dbContext.Songs
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<SongEntity>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Songs
            .AsNoTracking()
            .OrderBy(s => s.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<SongEntity> AddAsync(SongEntity song, CancellationToken cancellationToken = default)
    {
        dbContext.Songs.Add(song);
        await dbContext.SaveChangesAsync(cancellationToken);
        return song;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var rows = await dbContext.Songs
            .Where(s => s.Id == id)
            .ExecuteDeleteAsync(cancellationToken);
        return rows > 0;
    }
}
