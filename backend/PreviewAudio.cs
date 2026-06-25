using System.Security.Cryptography;
using System.Text;
using MusicStore.Music;

namespace MusicStore;

public sealed class PreviewAudio
{
    private readonly ToWav _toWav;
    private readonly LocaleStore _locales;
    private readonly string _cacheDirectory;

    public PreviewAudio(IHostEnvironment env, LocaleStore locales)
    {
        var soundFontPath = Path.Combine(env.ContentRootPath, "SoundFonts", "GeneralUser_GS.sf2");
        _locales = locales;
        _cacheDirectory = Path.Combine(env.ContentRootPath, ".cache", "audio");
        _toWav = new ToWav(soundFontPath);
        Directory.CreateDirectory(_cacheDirectory);
    }

    public async Task<byte[]> GetWav(string? localeCode, long userSeed, long globalIndex, int durationSeconds, CancellationToken ct = default)
    {
        var locale = _locales.Resolve(localeCode);
        var songSeed = SeedHash.SongSeed(userSeed, globalIndex, locale.Code);
        var cachePath = CachePath(songSeed, durationSeconds);

        if (File.Exists(cachePath))
        {
            try
            {
                return await File.ReadAllBytesAsync(cachePath, ct);
            }
            catch (IOException)
            {
            }
        }

        var track = Compose.Generate(songSeed, durationSeconds);
        var midi = ToMidi.Build(track);
        var wav = _toWav.Render(midi, durationSeconds + 0.5);

        await WriteAtomicAsync(cachePath, wav, ct);
        return wav;
    }

    private string CachePath(int songSeed, int durationSeconds)
    {
        var key = $"song_{songSeed}_dur{durationSeconds}";
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(key))).ToLowerInvariant();
        return Path.Combine(_cacheDirectory, $"preview_{hash}.wav");
    }

    private static async Task WriteAtomicAsync(string path, byte[] bytes, CancellationToken ct)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        var tempPath = $"{path}.{Guid.NewGuid():N}.tmp";
        await File.WriteAllBytesAsync(tempPath, bytes, ct);
        try
        {
            File.Move(tempPath, path, overwrite: true);
        }
        catch (IOException)
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }
}
