namespace MusicStore;

public class Song
{
    public long Index { get; set; }
    public string Title { get; set; } = "";
    public string Artist { get; set; } = "";
    public string Album { get; set; } = "";
    public string Genre { get; set; } = "";
    public int Likes { get; set; }
    public string Review { get; set; } = "";
    public string CoverUrl { get; set; } = "";
    public string PreviewUrl { get; set; } = "";
}

public class SongPage
{
    public required IReadOnlyList<Song> Items { get; init; }
    public required int Page { get; init; }
    public required bool HasMore { get; init; }
}

public class SongQuery
{
    public string? Lang { get; set; }
    public string? Seed { get; set; }
    public double Likes { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; } = 20;
}

public class AudioQuery
{
    public string? Seed { get; set; }
    public long Index { get; set; }
    public int Duration { get; set; } = 30;
}
