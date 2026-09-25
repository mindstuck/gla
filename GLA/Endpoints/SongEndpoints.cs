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

        // Serves the raw score file (Guitar Pro, MusicXML, ...) for rendering
        // with alphaTab on the client. 404 covers unknown songs, songs without
        // a file on disk, and paths escaping the files root.
        group.MapGet("/{id:int}/file", async (int id, ISongService songService, ScoreFileLocator fileLocator, CancellationToken cancellationToken) =>
        {
            var song = await songService.GetByIdAsync(id, cancellationToken);
            if (song is null)
            {
                return Results.NotFound();
            }

            var path = fileLocator.Locate(song.FilePath);
            return path is null
                ? Results.NotFound()
                : Results.File(path, ScoreFileContentTypes.FromPath(path));
        });
    }
}
