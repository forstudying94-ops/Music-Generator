using System.Text.Json;
using System.Text.Json.Serialization;

namespace MusicStore;

public class LocaleStore
{
    private readonly Dictionary<string, LocaleData> _byCode = new(StringComparer.OrdinalIgnoreCase);
    private readonly LocaleData _primary;

    public LocaleStore(IWebHostEnvironment env)
    {
        var folder = Path.Combine(env.ContentRootPath, "Locales");
        if (!Directory.Exists(folder))
            throw new DirectoryNotFoundException($"Locale folder not found: {folder}");

        var options = new JsonSerializerOptions(JsonSerializerOptions.Web)
        {
            PropertyNameCaseInsensitive = true
        };

        foreach (var file in Directory.EnumerateFiles(folder, "*.json", SearchOption.AllDirectories))
        {
            var json = File.ReadAllText(file);
            var data = JsonSerializer.Deserialize<LocaleData>(json, options);
            if (data is null || string.IsNullOrWhiteSpace(data.Code))
                continue;

            _byCode[data.Code] = data;
        }

        if (_byCode.Count == 0)
            throw new InvalidOperationException("No locale files could be loaded.");

        _primary = _byCode.Values.FirstOrDefault(l => l.IsPrimary)
            ?? _byCode.Values.OrderBy(l => l.Code).First();
    }

    public LocaleData Primary => _primary;

    public IReadOnlyList<LocaleSummary> List() =>
        _byCode.Values
            .OrderBy(l => l.Code)
            .Select(l => new LocaleSummary(l.Code, l.DisplayName))
            .ToList();

    public LocaleData Resolve(string? code)
    {
        if (!string.IsNullOrWhiteSpace(code) && _byCode.TryGetValue(code, out var match))
            return match;

        return _byCode.Values.First();
    }
}

public class LocaleData
{
    public string Code { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string FakerLocale { get; set; } = "";
    public bool IsPrimary { get; set; }
    public string SingleLabel { get; set; } = "Single";
    public List<string> Genres { get; set; } = new();
    public List<string> TitlePatterns { get; set; } = new();
    public List<string> AlbumPatterns { get; set; } = new();
    public List<string> BandPatterns { get; set; } = new();
    public List<string> Adjectives { get; set; } = new();
    public List<string> Nouns { get; set; } = new();
    public List<string> Verbs { get; set; } = new();
    public List<string> BandWords { get; set; } = new();
    public List<string> Reviews { get; set; } = new();
}

public record LocaleSummary(
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("displayName")] string DisplayName);
