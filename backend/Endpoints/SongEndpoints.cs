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

        // Upload: multipart/form-data with exactly one file part. Shape and
        // format validation lives here / in the service (400s below).
        group.MapPost("", async (HttpRequest request, ISongService songService, CancellationToken cancellationToken) =>
        {
            if (!request.HasFormContentType)
            {
                return Results.BadRequest(new { error = "Expected a multipart/form-data upload." });
            }

            var form = await request.ReadFormAsync(cancellationToken);
            if (form.Files.Count != 1)
            {
                return Results.BadRequest(new { error = "Upload exactly one file at a time." });
            }

            var upload = form.Files[0];
            if (upload.Length == 0)
            {
                return Results.BadRequest(new { error = "The file is empty." });
            }

            try
            {
                await using var content = upload.OpenReadStream();
                var song = await songService.CreateAsync(content, upload.FileName, cancellationToken);
                return Results.Created($"/api/songs/{song.Id}", song);
            }
            catch (UnsupportedScoreFileException e)
            {
                return Results.BadRequest(new { error = e.Message });
            }
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

        // Removes the song row and the score file it owns. 404 for unknown ids.
        group.MapDelete("/{id:int}", async (int id, ISongService songService, CancellationToken cancellationToken) =>
        {
            var deleted = await songService.DeleteAsync(id, cancellationToken);
            return deleted ? Results.NoContent() : Results.NotFound();
        });
    }
}
