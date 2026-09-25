using GLA.Services;

namespace GLA.Endpoints;

public static class SongEndpoints
{
    public static void MapSongEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/songs").WithTags("Songs");

        group.MapGet("", async (ISongService songService, CancellationToken cancellationToken) =>
        {
            var songs = await songService.ListAsync(cancellationToken);
            return Results.Ok(songs);
        });

        group.MapGet("/{id:int}", async (int id, ISongService songService, CancellationToken cancellationToken) =>
        {
            var song = await songService.GetByIdAsync(id, cancellationToken);
            return song is null ? Results.NotFound() : Results.Ok(song);
        });
    }
}
