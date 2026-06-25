using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace MusicStore.Covers;

public static class CoverRender
{
    public static byte[] Make(int songSeed, string title, string album, string artist)
    {
        var random = new Random(SeedHash.Mix(songSeed, "cover"));

        using var image = new Image<Rgba32>(CoverCanvas.Size, CoverCanvas.Size);

        var palette = CoverCanvas.PickPalette(random);
        CoverArt.DrawBackground(image, random, palette);
        CoverArt.DrawPattern(image, random, palette);
        CoverCanvas.DarkenBottom(image);
        CoverText.Draw(image, title, album, artist);

        using var stream = new MemoryStream();
        image.SaveAsPng(stream);
        return stream.ToArray();
    }
}

internal static class CoverCanvas
{
    internal const int Size = 512;

    internal static Rgba32[] PickPalette(Random random) => Palettes[random.Next(Palettes.Length)];

    internal static void DarkenBottom(Image<Rgba32> image)
    {
        int start = (int)(Size * 0.62f);
        for (var y = start; y < Size; y++)
        {
            float t = (y - start) / (float)(Size - start);
            float darken = t * t * 0.82f;
            for (var x = 0; x < Size; x++)
            {
                var p = image[x, y];
                image[x, y] = new Rgba32((byte)(p.R * (1 - darken)), (byte)(p.G * (1 - darken)), (byte)(p.B * (1 - darken)), p.A);
            }
        }
    }

    internal static void FillCircle(Image<Rgba32> image, int cx, int cy, int r, Rgba32 color, byte alpha)
    {
        for (var dy = -r; dy <= r; dy++)
        for (var dx = -r; dx <= r; dx++)
            if (dx * dx + dy * dy <= r * r)
                Blend(image, cx + dx, cy + dy, color, alpha);
    }

    internal static void Ring(Image<Rgba32> image, int cx, int cy, int r, Rgba32 color, byte alpha)
    {
        for (var dy = -(r + 1); dy <= r + 1; dy++)
        for (var dx = -(r + 1); dx <= r + 1; dx++)
        {
            var d = MathF.Sqrt(dx * dx + dy * dy);
            if (d >= r - 1 && d <= r + 1) Blend(image, cx + dx, cy + dy, color, alpha);
        }
    }

    internal static void Blend(Image<Rgba32> image, int x, int y, Rgba32 src, byte alpha)
    {
        if (alpha == 0 || x < 0 || x >= Size || y < 0 || y >= Size) return;
        var dst = image[x, y];
        float a = alpha / 255f, ia = 1f - a;
        image[x, y] = new Rgba32(
            (byte)(src.R * a + dst.R * ia),
            (byte)(src.G * a + dst.G * ia),
            (byte)(src.B * a + dst.B * ia),
            255);
    }

    internal static Rgba32 Lerp(Rgba32 a, Rgba32 b, float t)
    {
        t = Math.Clamp(t, 0f, 1f);
        return new Rgba32(
            (byte)(a.R + (b.R - a.R) * t),
            (byte)(a.G + (b.G - a.G) * t),
            (byte)(a.B + (b.B - a.B) * t),
            255);
    }

    private static readonly Rgba32[][] Palettes =
    [
        [Hex(0xFF6B35), Hex(0xF7C59F), Hex(0xEFEFD0)],
        [Hex(0x0D1B2A), Hex(0x1B4F72), Hex(0x5DADE2)],
        [Hex(0x0D0221), Hex(0x650D89), Hex(0xE800FF)],
        [Hex(0x2D4A22), Hex(0x6B8F4E), Hex(0xC8B560)],
        [Hex(0xC2185B), Hex(0xFF8A65), Hex(0xFFF3E0)],
        [Hex(0x0A192F), Hex(0x172A45), Hex(0x64FFDA)],
        [Hex(0x1A1A2E), Hex(0xE94560), Hex(0xF5A623)],
        [Hex(0x3D0066), Hex(0x7B00D4), Hex(0xE040FB)]
    ];

    private static Rgba32 Hex(uint rgb) => new((byte)(rgb >> 16), (byte)((rgb >> 8) & 0xFF), (byte)(rgb & 0xFF), 255);
}
