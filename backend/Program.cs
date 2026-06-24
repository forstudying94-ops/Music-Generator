using MusicStore;

var builder = WebApplication.CreateBuilder(args);

const string FrontendCors = "frontend";
var allowedOrigins = builder.Configuration["FrontendOrigins"]?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    ?? ["http://localhost:5173", "http://127.0.0.1:5173", "http://localhost:3000", "http://127.0.0.1:3000"];
builder.Services.AddCors(options =>
    options.AddPolicy(FrontendCors, policy =>
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddSingleton<LocaleStore>();
builder.Services.AddSingleton<SongGen>();
builder.Services.AddSingleton<PreviewAudio>();

var app = builder.Build();

app.UseCors(FrontendCors);

app.MapGet("/api/locales", (LocaleStore locales) => locales.List());

app.MapGet("/api/songs", (SongGen songs, [AsParameters] SongQuery query) =>
{
    var userSeed = Seed.GetSeed(query.Seed);
    var size = Math.Clamp(query.PageSize, 1, 100);
    var pageIndex = Math.Max(query.Page, 0);
    return Results.Ok(songs.Generate(query.Lang, userSeed, query.Likes, pageIndex, size));
});

app.MapGet("/api/cover", (SongGen songs, string? seed, long index) =>
{
    var userSeed = Seed.GetSeed(seed);
    var png = songs.RenderCover(userSeed, index);
    return Results.File(png, "image/png");
});

app.MapGet("/api/audio", async (PreviewAudio audio, [AsParameters] AudioQuery query, CancellationToken ct) =>
{
    var userSeed = Seed.GetSeed(query.Seed);
    var seconds = Math.Clamp(query.Duration, 5, 60);
    var wav = await audio.GetWav(userSeed, query.Index, seconds, ct);
    return Results.File(wav, "audio/wav");
});

app.Run();
