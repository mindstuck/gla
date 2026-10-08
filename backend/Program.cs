using GLA.Data;
using GLA.Endpoints;
using GLA.Repositories;
using GLA.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// SQLite: one file, no database server. The connection string comes from
// configuration ("GlaDb"); a relative Data Source is pinned to the content
// root by SqliteConnectionString below — SQLite itself would resolve it
// against the process working directory, which differs between `dotnet run`,
// `dotnet ef` and the container.
builder.Services.AddDbContext<GlaDbContext>(options =>
    options.UseSqlite(SqliteConnectionString(builder)));

builder.Services.AddScoped<ISongRepository, SongRepository>();
builder.Services.AddScoped<ISongService, SongService>();

// Score files live in a folder outside the web root (Songs:FilesRoot);
// relative paths are resolved against the content root (the GLA/ project
// folder in development, see appsettings.Development.json).
var filesRoot = builder.Configuration["Songs:FilesRoot"] ?? "storage";
if (!Path.IsPathRooted(filesRoot))
{
    filesRoot = Path.Combine(builder.Environment.ContentRootPath, filesRoot);
}
builder.Services.AddSingleton(new ScoreFileLocator(filesRoot));
builder.Services.AddSingleton(new SongFileStore(filesRoot));

// JSON: System.Text.Json with camelCase (ASP.NET Core defaults), configured
// explicitly so the contract is visible. The Vite dev proxy forwards /api to this
// origin, so CORS is not needed in development; this policy exists for direct
// cross-origin access (e.g. production client on a different host).
const string ApiCorsPolicy = "ApiClient";
builder.Services.AddCors(options =>
{
    options.AddPolicy(ApiCorsPolicy, policy => policy
        .WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
        .AllowAnyHeader()
        .AllowAnyMethod());
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseCors(ApiCorsPolicy);
app.UseHttpsRedirection();

// Create/upgrade the database and apply seed data on startup, in every
// environment: the schema exists only because of these migrations, and the
// seed songs are baked into them — a fresh production database (first
// `docker compose up`) comes out populated instead of erroring on /songs.
using var scope = app.Services.CreateScope();
await scope.ServiceProvider.GetRequiredService<GlaDbContext>().Database.MigrateAsync();

app.MapSongEndpoints();

app.Run();

// Resolves a relative "Data Source" against the content root and creates the
// containing folder. SQLite would otherwise resolve it against the process
// working directory (repo root vs backend/ vs /app in the container), and a
// fresh clone has no database folder yet.
static string SqliteConnectionString(WebApplicationBuilder builder)
{
    var connectionString = builder.Configuration.GetConnectionString("GlaDb")
        ?? "Data Source=gla.db";

    var dataSource = new SqliteConnectionStringBuilder(connectionString).DataSource;
    if (string.IsNullOrEmpty(dataSource) || dataSource == ":memory:" || Path.IsPathRooted(dataSource))
    {
        return connectionString;
    }

    var fullPath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, dataSource));
    Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

    return new SqliteConnectionStringBuilder(connectionString) { DataSource = fullPath }.ConnectionString;
}
