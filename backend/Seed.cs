namespace MusicStore;

public static class Seed
{
    private const long MulA = 6364136223846793005L;
    private const long MulB = 1442695040888963407L;
    private const long LikesSalt = unchecked((long)0x9E3779B97F4A7C15UL);

    public static int SongSeed(long userSeed, long globalIndex)
    {
        long mixed = unchecked(userSeed * MulA + globalIndex * MulB);
        return Fold(mixed);
    }

    public static int LikesSeed(long userSeed, long globalIndex)
    {
        long mixed = unchecked((userSeed ^ LikesSalt) * MulA + globalIndex * MulB);
        return Fold(mixed);
    }

    public static int Mix(int seed, string tag, int a = 0, int b = 0)
    {
        ulong hash = 14695981039346656037UL;
        hash = FnvStep(hash, unchecked((uint)seed));
        foreach (var ch in tag)
            hash = FnvStep(hash, ch);
        hash = FnvStep(hash, unchecked((uint)a));
        hash = FnvStep(hash, unchecked((uint)b));
        return unchecked((int)hash);
    }

    private static ulong FnvStep(ulong hash, uint value)
    {
        for (var i = 0; i < 4; i++)
        {
            hash ^= (byte)(value >> (i * 8));
            hash *= 1099511628211UL;
        }
        return hash;
    }

    private static int Fold(long value)
    {
        long x = unchecked(value ^ (long)((ulong)value >> 32));
        return (int)x;
    }

    public static long GetSeed(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return 0;

        if (long.TryParse(raw.Trim(), out var number))
            return number;

        ulong hash = 14695981039346656037UL;
        foreach (var ch in raw)
        {
            hash ^= ch;
            hash *= 1099511628211UL;
        }
        return unchecked((long)hash);
    }
}
