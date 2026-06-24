using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace MusicStore.Covers;

internal static class CoverArt
{
    private const int Size = CoverCanvas.Size;

    public static void DrawBackground(Image<Rgba32> image, Random random, Rgba32[] palette)
    {
        switch (random.Next(4))
        {
            case 0: LinearGradient(image, palette); break;
            case 1: RadialGradient(image, random, palette); break;
            case 2: SineField(image, random, palette); break;
            default: Sunburst(image, random, palette); break;
        }
    }

    public static void DrawPattern(Image<Rgba32> image, Random random, Rgba32[] palette)
    {
        switch (random.Next(4))
        {
            case 0: Bokeh(image, random, palette); break;
            case 1: Halftone(image, random, palette); break;
            case 2: Constellation(image, random); break;
            default: VinylGrooves(image, random, palette); break;
        }
    }

    private static void LinearGradient(Image<Rgba32> image, Rgba32[] palette)
    {
        for (var y = 0; y < Size; y++)
        {
            var color = CoverCanvas.Lerp(palette[0], palette[1], y / (float)(Size - 1));
            for (var x = 0; x < Size; x++) image[x, y] = color;
        }
    }

    private static void RadialGradient(Image<Rgba32> image, Random random, Rgba32[] palette)
    {
        float cx = Size * (0.3f + (float)random.NextDouble() * 0.4f);
        float cy = Size * (0.3f + (float)random.NextDouble() * 0.4f);
        float maxDist = Size * 0.8f;
        for (var y = 0; y < Size; y++)
        for (var x = 0; x < Size; x++)
        {
            var dist = MathF.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
            var t = Math.Clamp(dist / maxDist, 0f, 1f);
            image[x, y] = CoverCanvas.Lerp(palette[0], palette[1], t * t);
        }
    }

    private static void SineField(Image<Rgba32> image, Random random, Rgba32[] palette)
    {
        double fx = 2 + random.NextDouble() * 4, fy = 2 + random.NextDouble() * 4;
        double ox = random.NextDouble() * Math.PI * 2, oy = random.NextDouble() * Math.PI * 2;
        for (var y = 0; y < Size; y++)
        for (var x = 0; x < Size; x++)
        {
            double nx = x / (double)Size, ny = y / (double)Size;
            double v = (Math.Sin(nx * fx * Math.PI + ox) + Math.Sin(ny * fy * Math.PI + oy) +
                        Math.Sin((nx + ny) * 5 * Math.PI)) / 3.0;
            image[x, y] = CoverCanvas.Lerp(palette[0], palette[2 % palette.Length], (float)(v * 0.5 + 0.5));
        }
    }

    private static void Sunburst(Image<Rgba32> image, Random random, Rgba32[] palette)
    {
        float cx = Size / 2f, cy = Size / 2f;
        int rays = 8 + random.Next(12);
        for (var y = 0; y < Size; y++)
        for (var x = 0; x < Size; x++)
        {
            double angle = Math.Atan2(y - cy, x - cx) + Math.PI;
            double sector = angle / (Math.PI * 2) * rays;
            float blend = (float)(sector - (int)sector);
            var edge = MathF.Abs(blend - 0.5f) * 2f;
            var baseColor = CoverCanvas.Lerp(palette[1], palette[0], edge);
            float dist = MathF.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
            image[x, y] = CoverCanvas.Lerp(palette[2 % palette.Length], baseColor, Math.Clamp(dist / (Size * 0.6f), 0f, 1f));
        }
    }

    private static void Bokeh(Image<Rgba32> image, Random random, Rgba32[] palette)
    {
        var count = 14 + random.Next(18);
        for (var i = 0; i < count; i++)
        {
            int cx = random.Next(Size), cy = random.Next(Size), radius = 20 + random.Next(80);
            var color = palette[random.Next(palette.Length)];
            byte maxAlpha = (byte)(30 + random.Next(60));
            for (var dy = -radius; dy <= radius; dy++)
            for (var dx = -radius; dx <= radius; dx++)
            {
                var d = MathF.Sqrt(dx * dx + dy * dy);
                if (d > radius) continue;
                var falloff = 1f - d / radius;
                CoverCanvas.Blend(image, cx + dx, cy + dy, color, (byte)(maxAlpha * falloff * falloff));
            }
        }
    }

    private static void Halftone(Image<Rgba32> image, Random random, Rgba32[] palette)
    {
        var dot = palette[random.Next(palette.Length)];
        int spacing = 14 + random.Next(12);
        float maxRadius = spacing * 0.48f;
        float cx = Size / 2f, cy = Size / 2f;
        for (var gy = 0; gy <= Size / spacing + 1; gy++)
        for (var gx = 0; gx <= Size / spacing + 1; gx++)
        {
            int px = gx * spacing, py = gy * spacing;
            var dist = MathF.Sqrt((px - cx) * (px - cx) + (py - cy) * (py - cy));
            var density = 1f - Math.Clamp(dist / (Size * 0.7f), 0f, 1f);
            CoverCanvas.FillCircle(image, px, py, (int)(maxRadius * (0.2f + density * 0.8f)), dot, 150);
        }
    }

    private static void Constellation(Image<Rgba32> image, Random random)
    {
        var count = 70 + random.Next(90);
        for (var i = 0; i < count; i++)
        {
            int x = random.Next(Size), y = random.Next(Size);
            byte b = (byte)(180 + random.Next(75));
            CoverCanvas.FillCircle(image, x, y, random.Next(3) == 0 ? 2 : 1, new Rgba32(b, b, b), b);
        }
    }

    private static void VinylGrooves(Image<Rgba32> image, Random random, Rgba32[] palette)
    {
        float cx = Size / 2f, cy = Size / 2f;
        int grooves = 30 + random.Next(18);
        float maxR = Size * 0.48f, minR = Size * 0.06f;
        byte alpha = (byte)(35 + random.Next(40));
        for (var i = 0; i < grooves; i++)
        {
            float r = minR + (maxR - minR) * (i / (float)grooves);
            byte bright = (byte)(i % 2 == 0 ? 255 : 0);
            CoverCanvas.Ring(image, (int)cx, (int)cy, (int)r, new Rgba32(bright, bright, bright), alpha);
        }
        CoverCanvas.FillCircle(image, (int)cx, (int)cy, (int)(Size * 0.11f), palette[0], 200);
    }
}
