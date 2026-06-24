using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

namespace MusicStore.Music;

public static class ToMidi
{
    private const short TicksPerQuarterNote = 480;

    private static readonly FourBitNumber MelodyChannel = (FourBitNumber)0;
    private static readonly FourBitNumber ChordChannel = (FourBitNumber)1;
    private static readonly FourBitNumber BassChannel = (FourBitNumber)2;
    private static readonly FourBitNumber DrumChannel = (FourBitNumber)9; // GM percussion

    public static MidiFile Build(CompositionResult composition)
    {
        var midiFile = new MidiFile
        {
            TimeDivision = new TicksPerQuarterNoteTimeDivision(TicksPerQuarterNote)
        };

        var tempoTrack = new TrackChunk();
        using (var tempoManager = tempoTrack.ManageTimedEvents())
        {
            var microsecondsPerQuarter = (long)Math.Round(60_000_000.0 / composition.Tempo);
            tempoManager.Objects.Add(new TimedEvent(new SetTempoEvent(microsecondsPerQuarter)));
        }

        midiFile.Chunks.Add(tempoTrack);
        midiFile.Chunks.Add(InstrumentTrack(composition.MelodyNotes, MelodyChannel, composition.Profile.Instruments.MelodyProgram));
        midiFile.Chunks.Add(InstrumentTrack(composition.ChordNotes, ChordChannel, composition.Profile.Instruments.ChordProgram));
        midiFile.Chunks.Add(InstrumentTrack(composition.BassNotes, BassChannel, composition.Profile.Instruments.BassProgram));
        midiFile.Chunks.Add(DrumTrack(composition.DrumHits));

        return midiFile;
    }

    private static TrackChunk InstrumentTrack(IReadOnlyList<NoteEvent> noteEvents, FourBitNumber channel, int program)
    {
        var track = new TrackChunk();

        using (var events = track.ManageTimedEvents())
            events.Objects.Add(new TimedEvent(new ProgramChangeEvent((SevenBitNumber)program) { Channel = channel }, 0));

        using (var notes = track.ManageNotes())
        {
            foreach (var ev in noteEvents)
            {
                if (ev.MidiNote is < 0 or > 127) continue;

                var length = Math.Max(BeatsToTicks(ev.DurationBeats), 1);
                notes.Objects.Add(new Note(
                    (SevenBitNumber)Math.Clamp(ev.MidiNote, 0, 127),
                    length,
                    BeatsToTicks(ev.StartBeat))
                {
                    Channel = channel,
                    Velocity = (SevenBitNumber)Math.Clamp(ev.Velocity, 1, 127)
                });
            }
        }
        return track;
    }

    private static TrackChunk DrumTrack(IReadOnlyList<DrumHitEvent> drumHits)
    {
        var track = new TrackChunk();
        const long hitLength = 60;

        using (var notes = track.ManageNotes())
        {
            foreach (var hit in drumHits)
            {
                notes.Objects.Add(new Note(
                    (SevenBitNumber)Math.Clamp(hit.GmKey, 0, 127),
                    hitLength,
                    BeatsToTicks(hit.StartBeat))
                {
                    Channel = DrumChannel,
                    Velocity = (SevenBitNumber)Math.Clamp(hit.Velocity, 1, 127)
                });
            }
        }
        return track;
    }

    private static long BeatsToTicks(double beats) => (long)Math.Round(beats * TicksPerQuarterNote);
}
