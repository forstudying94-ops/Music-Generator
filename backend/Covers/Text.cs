using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace MusicStore.Covers;

internal static class CoverText
{
    private const int Size = CoverCanvas.Size;

    public static void Draw(Image<Rgba32> image, string title, string album, string artist)
    {
        const int pad = 28;
        int wrap = Size - pad * 2;

        var titleFont = GetFont(Size * 0.066f, FontStyle.Bold);
        var albumFont = GetFont(Size * 0.038f, FontStyle.Regular);
        var labelFont = GetFont(Size * 0.026f, FontStyle.Regular);

        var white = Color.FromRgba(255, 255, 255, 240);
        var offWhite = Color.FromRgba(225, 225, 225, 215);
        var accent = Color.FromRgba(255, 210, 100, 235);
        var shadow = Color.FromRgba(0, 0, 0, 180);

        var eyebrow = artist.ToUpperInvariant();
        if (eyebrow.Length > 30) eyebrow = eyebrow[..30] + "…";

        var eyebrowOpts = new RichTextOptions(labelFont) { Origin = new PointF(pad, Size - 124), WrappingLength = wrap };
        var titleOpts = new RichTextOptions(titleFont) { Origin = new PointF(pad, Size - 100), WrappingLength = wrap };
        var titleShadow = new RichTextOptions(titleFont) { Origin = new PointF(pad + 2, Size - 98), WrappingLength = wrap };
        var albumOpts = new RichTextOptions(albumFont) { Origin = new PointF(pad, Size - 42), WrappingLength = wrap };

        image.Mutate(ctx =>
        {
            ctx.DrawText(eyebrowOpts, eyebrow, accent);
            ctx.DrawText(titleShadow, title, shadow);
            ctx.DrawText(titleOpts, title, white);
            ctx.DrawText(albumOpts, album, offWhite);
        });
    }

    private static Font GetFont(float size, FontStyle style)
    {
        string[] candidates = ["Helvetica Neue", "Helvetica", "Arial", "Liberation Sans", "DejaVu Sans", "Verdana"];
        foreach (var name in candidates)
            if (SystemFonts.TryGet(name, out var family))
                return family.CreateFont(size, style);

        foreach (var path in FontFileCandidates())
        {
            if (!File.Exists(path)) continue;

            var collection = new FontCollection();
            return collection.Add(path).CreateFont(size, style);
        }

        if (SystemFonts.Families.Any())
            return SystemFonts.Families.First().CreateFont(size, style);

        throw new InvalidOperationException("No fonts available for cover rendering.");
    }

    private static IEnumerable<string> FontFileCandidates()
    {
        yield return Path.Combine(AppContext.BaseDirectory, "Fonts", "DejaVuSans.ttf");
        yield return "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf";
        yield return "/usr/share/fonts/TTF/DejaVuSans.ttf";
    }
}
