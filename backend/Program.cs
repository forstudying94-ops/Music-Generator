using MusicStore;

var builder = WebApplication.CreateBuilder(args);

const string FrontendCors = "frontend";
var allowedOrigins = ParseOrigins(builder.Configuration);
builder.Services.AddCors(options =>
    options.AddPolicy(FrontendCors, policy =>
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddSingleton<LocaleStore>();
builder.Services.AddSingleton<SongGenerator>();
builder.Services.AddSingleton<PreviewAudio>();

var app = builder.Build();

app.UseCors(FrontendCors);

app.MapGet("/api/locales", (LocaleStore locales) => locales.List());

app.MapGet("/api/songs", (SongGenerator songs, [AsParameters] SongQuery query) =>
{
    var userSeed = SeedHash.GetSeed(query.Seed);
    var size = Math.Clamp(query.PageSize, 1, 100);
    var pageIndex = Math.Max(query.Page, 0);
    return Results.Ok(songs.Generate(query.Lang, userSeed, query.Likes, pageIndex, size));
});

app.MapGet("/api/cover", (SongGenerator songs, string? seed, long index, string? lang) =>
{
    var userSeed = SeedHash.GetSeed(seed);
    var png = songs.RenderCover(lang, userSeed, index);
    return Results.File(png, "image/png");
});

app.MapGet("/api/audio", async (PreviewAudio audio, [AsParameters] AudioQuery query, ILogger<Program> logger, CancellationToken ct) =>
{
    try
    {
        var userSeed = SeedHash.GetSeed(query.Seed);
        var seconds = Math.Clamp(query.Duration, 5, 20);
        var wav = await audio.GetWav(query.Lang, userSeed, query.Index, seconds, ct);
        return Results.File(wav, "audio/wav");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Audio preview failed for seed={Seed} index={Index}", query.Seed, query.Index);
        return Results.Problem("Audio preview failed.", statusCode: StatusCodes.Status500InternalServerError);
    }
}).DisableRequestTimeout();

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapFallbackToFile("index.html");

app.Run();

static string[] ParseOrigins(IConfiguration config)
{
    var raw = config["FrontendOrigins"];
    if (string.IsNullOrWhiteSpace(raw))
    {
        return
        [
            "http://localhost:5173",
            "http://127.0.0.1:5173",
            "http://localhost:3000",
            "http://127.0.0.1:3000"
        ];
    }

    return raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Select(NormalizeOrigin)
        .ToArray();
}

static string NormalizeOrigin(string origin)
{
    origin = origin.Trim().TrimEnd('/');
    if (origin.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
        origin.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        return origin;

    return $"https://{origin}";
}
