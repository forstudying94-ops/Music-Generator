using MeltySynth;
using NAudio.Wave;

namespace MusicStore.Music;

public sealed class ToWav
{
    private const int PreviewSampleRate = 22050;

    private readonly SoundFont _soundFont;

    public ToWav(string soundFontPath)
    {
        if (!File.Exists(soundFontPath))
            throw new FileNotFoundException(
                $"SoundFont not found at '{soundFontPath}'. A .sf2 file is required for MeltySynth to render audio.",
                soundFontPath);

        _soundFont = new SoundFont(soundFontPath);
    }

    public byte[] Render(Melanchall.DryWetMidi.Core.MidiFile dryWetMidiFile, double maxSeconds)
    {
        using var midiStream = new MemoryStream();
        dryWetMidiFile.Write(midiStream, Melanchall.DryWetMidi.Core.MidiFileFormat.MultiTrack);
        midiStream.Position = 0;

        var meltyMidi = new MeltySynth.MidiFile(midiStream);

        var synthesizer = new Synthesizer(_soundFont, PreviewSampleRate);
        var sequencer = new MidiFileSequencer(synthesizer);
        sequencer.Play(meltyMidi, loop: false);

        var totalSeconds = Math.Min(meltyMidi.Length.TotalSeconds, maxSeconds);
        var totalSamples = (int)Math.Ceiling(totalSeconds * PreviewSampleRate) + PreviewSampleRate / 4;

        var left = new float[totalSamples];
        var right = new float[totalSamples];

        const int blockSize = 8192;
        var offset = 0;
        while (offset < totalSamples)
        {
            var count = Math.Min(blockSize, totalSamples - offset);
            sequencer.Render(new Span<float>(left, offset, count), new Span<float>(right, offset, count));
            offset += count;
        }

        return EncodeWav(left, right);
    }

    private static byte[] EncodeWav(float[] left, float[] right)
    {
        var format = new WaveFormat(PreviewSampleRate, 16, 2);
        using var memory = new MemoryStream();
        using (var writer = new WaveFileWriter(memory, format))
        {
            for (var i = 0; i < left.Length; i++)
            {
                writer.WriteSample(Math.Clamp(left[i], -1f, 1f));
                writer.WriteSample(Math.Clamp(right[i], -1f, 1f));
            }
        }
        return memory.ToArray();
    }
}
