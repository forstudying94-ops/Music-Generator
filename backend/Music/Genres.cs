



namespace MusicStore.Music;

public static class Genres
{
    private static readonly Dictionary<MusicGenre, GenreProfile> Profiles = new()
    {
        [MusicGenre.Rock] = Rock(),
        [MusicGenre.Pop] = Pop(),
        [MusicGenre.Electronic] = Electronic(),
        [MusicGenre.Jazz] = Jazz()
    };

    public static GenreProfile Get(MusicGenre genre) => Profiles[genre];

    public static MusicGenre Pick(int songSeed)
    {
        var values = Enum.GetValues<MusicGenre>();
        var rng = new Random(Seed.Mix(songSeed, "genre-pick"));
        return values[rng.Next(values.Length)];
    }

    private static GenreProfile Rock() => new()
    {
        Genre = MusicGenre.Rock,
        TempoRange = (100, 150),
        PossibleScales = [ScaleType.Major, ScaleType.NaturalMinor, ScaleType.Mixolydian],
        ProgressionPool = [[0, 4, 5, 3], [5, 3, 0, 4], [0, 3, 4, 3], [0, 5, 3, 4]],
        Instruments = new InstrumentSet { MelodyProgram = 29, ChordProgram = 30, BassProgram = 33 },
        DrumPatternPool = [RockDriving(), RockHalfTime()],
        SeventhChordBias = 0.10,
        SyncopationBias = 0.25,
        MelodyOctave = 5,
        BassOctave = 3
    };

    private static GenreProfile Pop() => new()
    {
        Genre = MusicGenre.Pop,
        TempoRange = (95, 128),
        PossibleScales = [ScaleType.Major, ScaleType.NaturalMinor],
        ProgressionPool = [[0, 4, 5, 3], [5, 3, 0, 4], [0, 3, 5, 4], [5, 4, 0, 4]],
        Instruments = new InstrumentSet { MelodyProgram = 80, ChordProgram = 4, BassProgram = 38 },
        DrumPatternPool = [PopFour(), PopSyncopated()],
        SeventhChordBias = 0.05,
        SyncopationBias = 0.35,
        MelodyOctave = 5,
        BassOctave = 3
    };

    private static GenreProfile Electronic() => new()
    {
        Genre = MusicGenre.Electronic,
        TempoRange = (118, 132),
        PossibleScales = [ScaleType.NaturalMinor, ScaleType.MinorPentatonic, ScaleType.Dorian],
        ProgressionPool = [[0, 0, 5, 3], [0, 5, 3, 4], [5, 3, 0, 0], [0, 3, 0, 4]],
        Instruments = new InstrumentSet { MelodyProgram = 81, ChordProgram = 89, BassProgram = 39 },
        DrumPatternPool = [FourOnFloor(), Breakbeat()],
        SeventhChordBias = 0.0,
        SyncopationBias = 0.45,
        MelodyOctave = 5,
        BassOctave = 2
    };

    private static GenreProfile Jazz() => new()
    {
        Genre = MusicGenre.Jazz,
        TempoRange = (80, 138),
        PossibleScales = [ScaleType.Dorian, ScaleType.Mixolydian, ScaleType.Major, ScaleType.HarmonicMinor],
        ProgressionPool = [[1, 4, 0, 5], [0, 5, 1, 4], [1, 4, 0, 0], [5, 1, 4, 0]],
        Instruments = new InstrumentSet { MelodyProgram = 65, ChordProgram = 0, BassProgram = 32 },
        DrumPatternPool = [JazzSwing()],
        SeventhChordBias = 0.85,
        SyncopationBias = 0.5,
        MelodyOctave = 5,
        BassOctave = 3
    };

    private static DrumPatternTemplate RockDriving() => new()
    {
        Name = "RockDriving",
        Voices =
        [
            new() { GmKey = GmDrums.BassDrum1, StepProbabilities = S(1,0,0,0, 0,0,1,0, 1,0,0,0, 0,0,1,0) },
            new() { GmKey = GmDrums.AcousticSnare, StepProbabilities = S(0,0,0,0, 1,0,0,0, 0,0,0,0, 1,0,0,0) },
            new() { GmKey = GmDrums.ClosedHiHat, StepProbabilities = S(1,1,1,1, 1,1,1,1, 1,1,1,1, 1,1,1,1) },
            new() { GmKey = GmDrums.CrashCymbal1, StepProbabilities = S(0.3,0,0,0, 0,0,0,0, 0,0,0,0, 0,0,0,0) }
        ]
    };

    private static DrumPatternTemplate RockHalfTime() => new()
    {
        Name = "RockHalfTime",
        Voices =
        [
            new() { GmKey = GmDrums.BassDrum1, StepProbabilities = S(1,0,0,0, 0,0,0,0, 0,0,1,0, 0,0,0,0) },
            new() { GmKey = GmDrums.AcousticSnare, StepProbabilities = S(0,0,0,0, 0,0,0,0, 1,0,0,0, 0,0,0,0) },
            new() { GmKey = GmDrums.ClosedHiHat, StepProbabilities = S(1,0,1,0, 1,0,1,0, 1,0,1,0, 1,0,1,0) }
        ]
    };

    private static DrumPatternTemplate PopFour() => new()
    {
        Name = "PopFour",
        Voices =
        [
            new() { GmKey = GmDrums.BassDrum1, StepProbabilities = S(1,0,0,0, 0,0,0,0.4, 1,0,0,0, 0,0,0,0) },
            new() { GmKey = GmDrums.ElectricSnare, StepProbabilities = S(0,0,0,0, 1,0,0,0, 0,0,0,0, 1,0,0,0) },
            new() { GmKey = GmDrums.ClosedHiHat, StepProbabilities = S(1,0,1,0, 1,0,1,0, 1,0,1,0, 1,0,1,0) }
        ]
    };

    private static DrumPatternTemplate PopSyncopated() => new()
    {
        Name = "PopSyncopated",
        Voices =
        [
            new() { GmKey = GmDrums.BassDrum1, StepProbabilities = S(1,0,0,0.6, 0,0,1,0, 0,0,0.5,0, 1,0,0,0) },
            new() { GmKey = GmDrums.ElectricSnare, StepProbabilities = S(0,0,0,0, 1,0,0,0, 0,0,0,0, 1,0,0,0.3) },
            new() { GmKey = GmDrums.ClosedHiHat, StepProbabilities = S(1,1,1,1, 1,1,1,1, 1,1,1,1, 1,1,1,1) }
        ]
    };

    private static DrumPatternTemplate FourOnFloor() => new()
    {
        Name = "FourOnFloor",
        Voices =
        [
            new() { GmKey = GmDrums.BassDrum1, StepProbabilities = S(1,0,0,0, 1,0,0,0, 1,0,0,0, 1,0,0,0) },
            new() { GmKey = GmDrums.ElectricSnare, StepProbabilities = S(0,0,0,0, 0,0,0,0, 0,0,0,0, 1,0,0,0) },
            new() { GmKey = GmDrums.OpenHiHat, StepProbabilities = S(0,0,1,0, 0,0,1,0, 0,0,1,0, 0,0,1,0) },
            new() { GmKey = GmDrums.ClosedHiHat, StepProbabilities = S(0,1,0,1, 0,1,0,1, 0,1,0,1, 0,1,0,1) }
        ]
    };

    private static DrumPatternTemplate Breakbeat() => new()
    {
        Name = "Breakbeat",
        Voices =
        [
            new() { GmKey = GmDrums.BassDrum1, StepProbabilities = S(1,0,0,0.5, 0,0,1,0, 0,0.4,0,0, 1,0,0,0) },
            new() { GmKey = GmDrums.ElectricSnare, StepProbabilities = S(0,0,0,0, 1,0,0,0.3, 0,0,0,0.4, 1,0,0,0) },
            new() { GmKey = GmDrums.ClosedHiHat, StepProbabilities = S(1,1,1,1, 1,1,1,1, 1,1,1,1, 1,1,1,1) }
        ]
    };

    private static DrumPatternTemplate JazzSwing() => new()
    {
        Name = "JazzSwing",
        Voices =
        [
            new() { GmKey = GmDrums.RideCymbal1, StepProbabilities = S(1,0,0.7,0, 1,0,0.7,0, 1,0,0.7,0, 1,0,0.7,0) },
            new() { GmKey = GmDrums.BassDrum1, StepProbabilities = S(0.4,0,0,0, 0,0,0.3,0, 0.4,0,0,0, 0,0,0.3,0) },
            new() { GmKey = GmDrums.AcousticSnare, StepProbabilities = S(0,0,0,0.3, 0,0,0,0, 0,0,0,0.3, 0,0,0,0) }
        ]
    };

    private static double[] S(params double[] steps) => steps;
}
