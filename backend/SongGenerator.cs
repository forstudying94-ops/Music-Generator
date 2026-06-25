using System.Text.RegularExpressions;
using Bogus;
using MusicStore.Covers;

namespace MusicStore;

public class SongGenerator
{
    private readonly LocaleStore _locales;
    private static readonly Regex Token = new(@"\{(\w+)\}", RegexOptions.Compiled);

    public SongGenerator(LocaleStore locales) => _locales = locales;

    public SongPage Generate(string? localeCode, long userSeed, double avgLikes, int page, int pageSize)
    {
        avgLikes = Math.Clamp(avgLikes, 0, 10);

        var items = new List<Song>(pageSize);
        for (var i = 0; i < pageSize; i++)
        {
            var globalIndex = (long)page * pageSize + i + 1;
            items.Add(GenerateOne(localeCode, userSeed, globalIndex, avgLikes));
        }

        return new SongPage { Items = items, Page = page, HasMore = true };
    }

    public Song GenerateOne(string? localeCode, long userSeed, long globalIndex, double avgLikes)
    {
        var locale = _locales.Resolve(localeCode);
        var primary = _locales.Primary;
        var songSeed = SeedHash.SongSeed(userSeed, globalIndex, locale.Code);

        var primaryFaker = MakeFaker(primary, songSeed);
        var localeFaker = MakeFaker(locale, SeedHash.Mix(songSeed, locale.Code));

        var title = Fill(primaryFaker.Random.ListItem(primary.TitlePatterns), primaryFaker, primary);

        return new Song
        {
            Index = globalIndex,
            Title = title,
            Artist = BuildArtist(primaryFaker, primary),
            Album = BuildAlbum(primaryFaker, primary),
            Genre = localeFaker.Random.ListItem(locale.Genres),
            Review = localeFaker.Random.ListItem(locale.Reviews),
            Likes = RollLikes(songSeed, Math.Clamp(avgLikes, 0, 10)),
            CoverUrl = $"/api/cover?seed={userSeed}&index={globalIndex}&lang={Uri.EscapeDataString(locale.Code)}",
            PreviewUrl = $"/api/audio?seed={userSeed}&index={globalIndex}&lang={Uri.EscapeDataString(locale.Code)}&duration=8"
        };
    }

    public byte[] RenderCover(string? localeCode, long userSeed, long globalIndex)
    {
        var locale = _locales.Resolve(localeCode);
        var song = GenerateOne(locale.Code, userSeed, globalIndex, 0);
        var songSeed = SeedHash.SongSeed(userSeed, globalIndex, locale.Code);
        return CoverRender.Make(songSeed, song.Title, song.Album, song.Artist);
    }

    private static Faker MakeFaker(LocaleData locale, int seed) =>
        new(locale.FakerLocale) { Random = new Randomizer(seed) };

    private string BuildArtist(Faker faker, LocaleData data)
    {
        if (faker.Random.Bool(0.55f))
            return Fill(faker.Random.ListItem(data.BandPatterns), faker, data);

        return faker.Name.FullName();
    }

    private string BuildAlbum(Faker faker, LocaleData data)
    {
        if (faker.Random.Bool(0.30f))
            return data.SingleLabel;

        return Fill(faker.Random.ListItem(data.AlbumPatterns), faker, data);
    }

    private string Fill(string pattern, Faker faker, LocaleData data) =>
        Token.Replace(pattern, match => match.Groups[1].Value switch
        {
            "adjective" => faker.Random.ListItem(data.Adjectives),
            "noun" => faker.Random.ListItem(data.Nouns),
            "verb" => faker.Random.ListItem(data.Verbs),
            "bandWord" => faker.Random.ListItem(data.BandWords),
            "number" => faker.Random.Int(1, 99).ToString(),
            _ => match.Value
        });

    private static int RollLikes(int songSeed, double avgLikes)
    {
        if (avgLikes <= 0) return 0;

        var likesRng = new Randomizer(SeedHash.Mix(songSeed, "likes"));
        var baseLikes = (int)Math.Floor(avgLikes);
        var chanceOfExtra = avgLikes - baseLikes;
        var extra = likesRng.Double() < chanceOfExtra ? 1 : 0;
        return Math.Clamp(baseLikes + extra, 0, 10);
    }
}
