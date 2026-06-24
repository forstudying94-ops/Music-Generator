namespace MusicStore.Music;

public enum MusicGenre
{
    Rock,
    Pop,
    Electronic,
    Jazz
}

public static class GmDrums
{
    public const int BassDrum1 = 36;
    public const int AcousticSnare = 38;
    public const int ElectricSnare = 40;
    public const int ClosedHiHat = 42;
    public const int OpenHiHat = 46;
    public const int CrashCymbal1 = 49;
    public const int RideCymbal1 = 51;
}

public sealed class NoteEvent
{
    public required int MidiNote { get; init; }
    public required double StartBeat { get; init; }
    public required double DurationBeats { get; init; }
    public required int Velocity { get; init; }
}

public sealed class DrumHitEvent
{
    public required int GmKey { get; init; }
    public required double StartBeat { get; init; }
    public required int Velocity { get; init; }
}

public sealed class InstrumentSet
{
    public required int MelodyProgram { get; init; }
    public required int ChordProgram { get; init; }
    public required int BassProgram { get; init; }
}

public sealed class DrumVoiceTemplate
{
    public required int GmKey { get; init; }
    public required double[] StepProbabilities { get; init; } // 16 sixteenth-note steps
}

public sealed class DrumPatternTemplate
{
    public required string Name { get; init; }
    public required IReadOnlyList<DrumVoiceTemplate> Voices { get; init; }
}

public sealed class GenreProfile
{
    public required MusicGenre Genre { get; init; }
    public required (int Min, int Max) TempoRange { get; init; }
    public required IReadOnlyList<ScaleType> PossibleScales { get; init; }
    public required IReadOnlyList<int[]> ProgressionPool { get; init; }
    public required InstrumentSet Instruments { get; init; }
    public required IReadOnlyList<DrumPatternTemplate> DrumPatternPool { get; init; }
    public required double SeventhChordBias { get; init; }
    public required double SyncopationBias { get; init; }
    public required int MelodyOctave { get; init; }
    public required int BassOctave { get; init; }
}

public sealed class CompositionResult
{
    public required MusicGenre Genre { get; init; }
    public required GenreProfile Profile { get; init; }
    public required MusicKey Key { get; init; }
    public required int Tempo { get; init; }
    public required int TotalBars { get; init; }
    public required int BeatsPerBar { get; init; }
    public required IReadOnlyList<Chord> ChordProgression { get; init; }
    public required IReadOnlyList<NoteEvent> MelodyNotes { get; init; }
    public required IReadOnlyList<NoteEvent> ChordNotes { get; init; }
    public required IReadOnlyList<NoteEvent> BassNotes { get; init; }
    public required IReadOnlyList<DrumHitEvent> DrumHits { get; init; }
}

public enum ScaleType
{
    Major,
    NaturalMinor,
    Dorian,
    Mixolydian,
    MinorPentatonic,
    HarmonicMinor
}

public enum ChordQuality
{
    Major,
    Minor,
    Diminished,
    Augmented,
    Dominant7,
    Major7,
    Minor7,
    HalfDiminished7
}

public sealed class MusicKey
{
    public int TonicPitchClass { get; }
    public ScaleType Scale { get; }
    private readonly int[] _intervals;

    public MusicKey(int tonicPitchClass, ScaleType scale)
    {
        TonicPitchClass = ((tonicPitchClass % 12) + 12) % 12;
        Scale = scale;
        _intervals = ScaleIntervals(scale);
    }

    public int Degrees => _intervals.Length;

    public int PitchClassForDegree(int degree)
    {
        var degreeInOctave = ((degree % _intervals.Length) + _intervals.Length) % _intervals.Length;
        return (TonicPitchClass + _intervals[degreeInOctave]) % 12;
    }

    public int MidiNoteForDegree(int degree, int octave)
    {
        var octaveOffset = (int)Math.Floor((double)degree / _intervals.Length);
        var degreeInOctave = degree - (octaveOffset * _intervals.Length);
        var semitoneFromTonic = _intervals[degreeInOctave] + (octaveOffset * 12);
        return ((octave + 1) * 12) + TonicPitchClass + semitoneFromTonic;
    }

    public bool Contains(int midiNote)
    {
        var pc = ((midiNote % 12) + 12) % 12;
        foreach (var interval in _intervals)
            if (((TonicPitchClass + interval) % 12) == pc)
                return true;
        return false;
    }

    public int SnapToScale(int midiNote)
    {
        if (Contains(midiNote)) return midiNote;
        for (var distance = 1; distance <= 6; distance++)
        {
            if (Contains(midiNote - distance)) return midiNote - distance;
            if (Contains(midiNote + distance)) return midiNote + distance;
        }
        return midiNote;
    }

    private static int[] ScaleIntervals(ScaleType scale) => scale switch
    {
        ScaleType.Major => [0, 2, 4, 5, 7, 9, 11],
        ScaleType.NaturalMinor => [0, 2, 3, 5, 7, 8, 10],
        ScaleType.Dorian => [0, 2, 3, 5, 7, 9, 10],
        ScaleType.Mixolydian => [0, 2, 4, 5, 7, 9, 10],
        ScaleType.MinorPentatonic => [0, 3, 5, 7, 10],
        ScaleType.HarmonicMinor => [0, 2, 3, 5, 7, 8, 11],
        _ => throw new ArgumentOutOfRangeException(nameof(scale))
    };

    public ChordQuality DiatonicQualityForDegree(int degree)
    {
        var d = ((degree % Degrees) + Degrees) % Degrees;
        return Scale switch
        {
            ScaleType.Major => d switch { 0 => ChordQuality.Major, 1 => ChordQuality.Minor, 2 => ChordQuality.Minor, 3 => ChordQuality.Major, 4 => ChordQuality.Major, 5 => ChordQuality.Minor, _ => ChordQuality.Diminished },
            ScaleType.NaturalMinor => d switch { 0 => ChordQuality.Minor, 1 => ChordQuality.Diminished, 2 => ChordQuality.Major, 3 => ChordQuality.Minor, 4 => ChordQuality.Minor, 5 => ChordQuality.Major, _ => ChordQuality.Major },
            ScaleType.Dorian => d switch { 0 => ChordQuality.Minor, 1 => ChordQuality.Minor, 2 => ChordQuality.Major, 3 => ChordQuality.Major, 4 => ChordQuality.Minor, 5 => ChordQuality.Diminished, _ => ChordQuality.Major },
            ScaleType.Mixolydian => d switch { 0 => ChordQuality.Major, 1 => ChordQuality.Minor, 2 => ChordQuality.Diminished, 3 => ChordQuality.Major, 4 => ChordQuality.Minor, 5 => ChordQuality.Minor, _ => ChordQuality.Major },
            ScaleType.HarmonicMinor => d switch { 0 => ChordQuality.Minor, 1 => ChordQuality.Diminished, 2 => ChordQuality.Augmented, 3 => ChordQuality.Minor, 4 => ChordQuality.Major, 5 => ChordQuality.Major, _ => ChordQuality.Diminished },
            _ => ChordQuality.Major
        };
    }

    public static int[] ChordToneIntervals(ChordQuality quality) => quality switch
    {
        ChordQuality.Major => [0, 4, 7],
        ChordQuality.Minor => [0, 3, 7],
        ChordQuality.Diminished => [0, 3, 6],
        ChordQuality.Augmented => [0, 4, 8],
        ChordQuality.Dominant7 => [0, 4, 7, 10],
        ChordQuality.Major7 => [0, 4, 7, 11],
        ChordQuality.Minor7 => [0, 3, 7, 10],
        ChordQuality.HalfDiminished7 => [0, 3, 6, 10],
        _ => [0, 4, 7]
    };
}

public sealed class Chord
{
    public int RootPitchClass { get; }
    public ChordQuality Quality { get; }
    public int ScaleDegree { get; }
    public IReadOnlyList<int> TonePitchClasses { get; }

    public Chord(int rootPitchClass, ChordQuality quality, int scaleDegree)
    {
        RootPitchClass = ((rootPitchClass % 12) + 12) % 12;
        Quality = quality;
        ScaleDegree = scaleDegree;
        TonePitchClasses = MusicKey.ChordToneIntervals(quality)
            .Select(interval => (RootPitchClass + interval) % 12)
            .ToList();
    }

    public int ToneNearOctave(int toneIndex, int anchorMidiNote)
    {
        var pitchClass = TonePitchClasses[((toneIndex % TonePitchClasses.Count) + TonePitchClasses.Count) % TonePitchClasses.Count];
        var candidate = (anchorMidiNote / 12 * 12) + pitchClass;

        var best = candidate;
        var bestDistance = Math.Abs(candidate - anchorMidiNote);
        foreach (var delta in (int[])[-24, -12, 12, 24])
        {
            var alt = candidate + delta;
            var dist = Math.Abs(alt - anchorMidiNote);
            if (dist < bestDistance) { best = alt; bestDistance = dist; }
        }
        return best;
    }
}
