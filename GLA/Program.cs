using GLA.Data;
using GLA.Endpoints;
using GLA.Repositories;
using GLA.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<GlaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("GlaDb")));

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

if (app.Environment.IsDevelopment())
{
    // Create/upgrade the database and apply seed data on startup.
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<GlaDbContext>().Database.MigrateAsync();
}

app.MapSongEndpoints();

app.Run();
