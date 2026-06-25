

namespace MusicStore.Music;

public static class Compose
{
    private const int BeatsPerBar = 4;
    private const int StepsPerBar = 16;

    private static readonly double[][] MelodyRhythms =
    [
        [4.0], [2.0, 2.0], [2.0, 1.0, 1.0], [1.0, 1.0, 2.0],
        [1.0, 1.0, 1.0, 1.0], [1.5, 1.5, 1.0],
        [0.5, 0.5, 1.0, 1.0, 1.0], [1.0, 0.5, 0.5, 1.0, 1.0]
    ];

    private static readonly double[][] BassRhythms =
    [
        [4.0], [2.0, 2.0], [1.0, 1.0, 1.0, 1.0],
        [1.0, 1.0, 2.0], [0.5, 0.5, 1.0, 1.0, 1.0], [1.0, 0.5, 0.5, 2.0]
    ];

    public static CompositionResult Generate(int songSeed, int targetDurationSeconds)
    {
        var genre = Genres.Pick(songSeed);
        var profile = Genres.Get(genre);

        var key = SelectKey(profile, songSeed);
        var tempo = SelectTempo(profile, songSeed);
        var totalBars = BarCount(tempo, targetDurationSeconds);

        var progression = BuildProgression(profile, key, songSeed, totalBars);

        return new CompositionResult
        {
            Genre = genre,
            Profile = profile,
            Key = key,
            Tempo = tempo,
            TotalBars = totalBars,
            BeatsPerBar = BeatsPerBar,
            ChordProgression = progression,
            MelodyNotes = BuildMelody(profile, key, progression, songSeed),
            ChordNotes = BuildChordPad(profile, progression, songSeed),
            BassNotes = BuildBass(profile, key, progression, songSeed),
            DrumHits = BuildDrums(profile, songSeed, totalBars)
        };
    }

    private static MusicKey SelectKey(GenreProfile profile, int songSeed)
    {
        var rng = new Random(SeedHash.Mix(songSeed, "key"));
        return new MusicKey(rng.Next(12), profile.PossibleScales[rng.Next(profile.PossibleScales.Count)]);
    }

    private static int SelectTempo(GenreProfile profile, int songSeed)
    {
        var rng = new Random(SeedHash.Mix(songSeed, "tempo"));
        return rng.Next(profile.TempoRange.Min, profile.TempoRange.Max + 1);
    }

    private static int BarCount(int tempo, int targetDurationSeconds)
    {
        var secondsPerBar = (60.0 / tempo) * BeatsPerBar;
        var rawBars = targetDurationSeconds / secondsPerBar;
        var bars = Math.Max((int)Math.Ceiling(rawBars), 2);
        return bars + (bars % 2);
    }

    private static IReadOnlyList<Chord> BuildProgression(GenreProfile profile, MusicKey key, int songSeed, int totalBars)
    {
        var pickRng = new Random(SeedHash.Mix(songSeed, "progression-pick"));
        var pattern = profile.ProgressionPool[pickRng.Next(profile.ProgressionPool.Count)];
        var seventhRng = new Random(SeedHash.Mix(songSeed, "progression-sevenths"));

        var chords = new List<Chord>(totalBars);
        for (var bar = 0; bar < totalBars; bar++)
        {
            var degree = pattern[bar % pattern.Length];
            var quality = key.DiatonicQualityForDegree(degree);
            if (seventhRng.NextDouble() < profile.SeventhChordBias)
                quality = ToSeventh(quality);
            chords.Add(new Chord(key.PitchClassForDegree(degree), quality, degree));
        }
        return chords;
    }

    private static ChordQuality ToSeventh(ChordQuality triad) => triad switch
    {
        ChordQuality.Major => ChordQuality.Major7,
        ChordQuality.Minor => ChordQuality.Minor7,
        ChordQuality.Diminished => ChordQuality.HalfDiminished7,
        _ => triad
    };

    private static IReadOnlyList<NoteEvent> BuildMelody(GenreProfile profile, MusicKey key, IReadOnlyList<Chord> progression, int songSeed)
    {
        var notes = new List<NoteEvent>();
        var anchor = key.MidiNoteForDegree(0, profile.MelodyOctave);
        var previous = anchor;

        for (var bar = 0; bar < progression.Count; bar++)
        {
            var chord = progression[bar];
            var rhythmRng = new Random(SeedHash.Mix(songSeed, "melody-rhythm", bar));
            var pitchRng = new Random(SeedHash.Mix(songSeed, "melody-pitch", bar));
            var velRng = new Random(SeedHash.Mix(songSeed, "melody-velocity", bar));
            var restRng = new Random(SeedHash.Mix(songSeed, "melody-rest", bar));

            var pattern = MelodyRhythms[rhythmRng.Next(MelodyRhythms.Length)];
            var cursor = 0.0;

            for (var n = 0; n < pattern.Length; n++)
            {
                var duration = pattern[n];
                var startBeat = bar * BeatsPerBar + cursor;
                var isRest = n > 0 && restRng.NextDouble() < 0.12;

                if (!isRest)
                {
                    var strong = n == 0 || Math.Abs(cursor % (BeatsPerBar / 2.0)) < 0.01;
                    int pitch;
                    if (strong)
                        pitch = chord.ToneNearOctave(pitchRng.Next(chord.TonePitchClasses.Count), previous);
                    else
                        pitch = key.SnapToScale(previous + pitchRng.Next(-2, 3));

                    pitch = Wrap(pitch, anchor - 7, anchor + 12);
                    var velocity = Math.Clamp(70 + velRng.Next(-8, 18) + (strong ? 8 : 0), 1, 127);

                    notes.Add(new NoteEvent { MidiNote = pitch, StartBeat = startBeat, DurationBeats = duration * 0.95, Velocity = velocity });
                    previous = pitch;
                }
                cursor += duration;
            }
        }
        return notes;
    }

    private static IReadOnlyList<NoteEvent> BuildChordPad(GenreProfile profile, IReadOnlyList<Chord> progression, int songSeed)
    {
        var notes = new List<NoteEvent>();
        var anchor = (profile.MelodyOctave - 1 + 1) * 12; // tonic-ish register, one octave below melody
        var velRng = new Random(SeedHash.Mix(songSeed, "chord-velocity"));

        for (var bar = 0; bar < progression.Count; bar++)
        {
            var chord = progression[bar];
            var startBeat = bar * BeatsPerBar;
            for (var tone = 0; tone < chord.TonePitchClasses.Count; tone++)
            {
                var pitch = chord.ToneNearOctave(tone, anchor);
                var velocity = Math.Clamp(48 + velRng.Next(-5, 6), 1, 127);
                notes.Add(new NoteEvent { MidiNote = pitch, StartBeat = startBeat, DurationBeats = BeatsPerBar * 0.92, Velocity = velocity });
            }
        }
        return notes;
    }

    private static IReadOnlyList<NoteEvent> BuildBass(GenreProfile profile, MusicKey key, IReadOnlyList<Chord> progression, int songSeed)
    {
        var notes = new List<NoteEvent>();
        var anchor = key.MidiNoteForDegree(0, profile.BassOctave);

        for (var bar = 0; bar < progression.Count; bar++)
        {
            var chord = progression[bar];
            var rhythmRng = new Random(SeedHash.Mix(songSeed, "bass-rhythm", bar));
            var pitchRng = new Random(SeedHash.Mix(songSeed, "bass-pitch", bar));
            var velRng = new Random(SeedHash.Mix(songSeed, "bass-velocity", bar));
            var syncRng = new Random(SeedHash.Mix(songSeed, "bass-sync", bar));

            var pattern = BassRhythms[rhythmRng.Next(BassRhythms.Length)];
            var cursor = 0.0;

            for (var n = 0; n < pattern.Length; n++)
            {
                var duration = pattern[n];
                var startBeat = bar * BeatsPerBar + cursor;

                int pitch;
                if (n == 0)
                    pitch = chord.ToneNearOctave(0, anchor);
                else if (syncRng.NextDouble() < profile.SyncopationBias * 0.3)
                    pitch = key.SnapToScale(chord.ToneNearOctave(0, anchor) - 1);
                else
                    pitch = chord.ToneNearOctave(pitchRng.NextDouble() < 0.7 ? 0 : 2 % chord.TonePitchClasses.Count, anchor);

                var velocity = Math.Clamp(78 + velRng.Next(-6, 10), 1, 127);
                notes.Add(new NoteEvent { MidiNote = pitch, StartBeat = startBeat, DurationBeats = duration * 0.9, Velocity = velocity });
                cursor += duration;
            }
        }
        return notes;
    }

    private static IReadOnlyList<DrumHitEvent> BuildDrums(GenreProfile profile, int songSeed, int totalBars)
    {
        var templateRng = new Random(SeedHash.Mix(songSeed, "drum-template"));
        var template = profile.DrumPatternPool[templateRng.Next(profile.DrumPatternPool.Count)];

        var hits = new List<DrumHitEvent>();
        var beatsPerStep = BeatsPerBar / (double)StepsPerBar;

        foreach (var voice in template.Voices)
        {
            for (var bar = 0; bar < totalBars; bar++)
            {
                var voiceRng = new Random(SeedHash.Mix(songSeed, "drum-voice", voice.GmKey, bar));
                var velRng = new Random(SeedHash.Mix(songSeed, "drum-velocity", voice.GmKey, bar));

                for (var step = 0; step < StepsPerBar; step++)
                {
                    var probability = voice.StepProbabilities[step];
                    if (probability <= 0.0 || voiceRng.NextDouble() >= probability)
                        continue;

                    var startBeat = bar * BeatsPerBar + step * beatsPerStep;
                    var accent = step % 4 == 0 ? 12 : 0;
                    var velocity = Math.Clamp(75 + accent + velRng.Next(-10, 11), 1, 127);
                    hits.Add(new DrumHitEvent { GmKey = voice.GmKey, StartBeat = startBeat, Velocity = velocity });
                }
            }
        }
        return hits;
    }

    private static int Wrap(int pitch, int min, int max)
    {
        while (pitch < min) pitch += 12;
        while (pitch > max) pitch -= 12;
        return pitch;
    }
}
