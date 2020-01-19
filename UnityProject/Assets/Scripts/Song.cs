//
//  a song is an audio file paired with a midi file (which contains tempo, and note events)
//

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;

public class Song : MonoBehaviour
{

   [Header("Basic Config")]
   public AudioClip AudioToPlay;
   public bool Loop = true;
   [Tooltip("path to a .mid file, expected to be relative to the StreamingAssets folder")]
   public string PathToMidiFile = "midi/test.mid";
   public float LengthInBeats = 8.0f;

   [Header("Note Authoring Config")]
   [Tooltip("(optional)A track name to look for the authoring. if not set, we use the first track we find in the midi file")]
   public string AuthoringTrackName = "";
   public int BaseInstrumentNote = 60;

   [Header("Progression Stuff")]
   [Tooltip("How many intersitital screens does the player click thru before gameplay starts")]
   public int NumPregameplayIntertitials = 1;

   [Header("TUBS Config")]
   public float BeatsPerChunk = 1.0f;

   public struct Mbt
   {
      public Mbt(int m, int b, int t) { measure = m; beat = b; tick = t; }
      public int measure;
      public int beat;
      public int tick;

      public override string ToString()
      {
         return measure + "." + beat + "." + tick;
      }
   };

   public class AuthoringEvent
   {
      public int NoteIdx = 0; //note relative to BaseInstrumentNote
      public float NoteOnBeat = -1.0f;
      public float NoteOffBeat = -1.0f;

      public bool IsValid()
      {
         return (NoteOnBeat >= 0.0f) && (NoteOffBeat >= 0.0f) && (NoteOffBeat > NoteOnBeat);
      }
   }

   List<AuthoringEvent> _authoring = new List<AuthoringEvent>();

   AudioSource _source = null; //filled in when we are playing

   private MidiFile _midiFile;
   private TempoMap _tempoMap = null;
   private string _loadedMidiPath = "";

   public List<AuthoringEvent> GetAuthoring()
   {
      return _authoring;
   }

   public bool IsPlaying()
   {
      return _source ? _source.isPlaying : false;
   }

   public void Prepare()
   {
      _LoadMidiFile(PathToMidiFile);
   }

   public void Play(AudioSource source)
   {
      _source = source;

      source.clip = AudioToPlay;
      source.loop = Loop;
      source.Play();

      //load midi file
      if (!_HasMidiFile())
         _LoadMidiFile(PathToMidiFile);

      //hook it up to master timeline to expose time
      if (MasterTimeline.I)
         MasterTimeline.I.SongToSyncWith = this;
   }

   public void Stop()
   {
      if (!IsPlaying())
         return;

      _source.Stop();
   }

   public int GetNumInstrumentPads()
   {
      int numRows = 0;
      foreach (var e in _authoring)
      {
         int row = e.NoteIdx + 1;
         if (row > numRows)
            numRows = row;
      }

      return numRows;
   }

   public bool IsNoteOnBetween(float startBeat, float endBeat, int noteIdx)
   {
      var auth = GetAuthoring();
      if (auth == null)
         return false;

      foreach (var e in auth)
      {
         if ((e.NoteOnBeat >= startBeat) && (e.NoteOnBeat < endBeat) && (e.NoteIdx == noteIdx))
            return true;
      }

      return false;
   }

   TrackChunk _FindAuthoringTracks()
   {
      if (!_HasMidiFile())
         return null;

      //look specificall for a named track
      if (AuthoringTrackName.Length > 0)
      {
         foreach (TrackChunk c in _midiFile.Chunks)
         {
            var trackName = c.Events
                             .OfType<SequenceTrackNameEvent>()
                             .FirstOrDefault()
                             ?.Text;
            if (trackName == AuthoringTrackName)
               return c;
         }
      }

      //just return last track (don't want the first one, since its typically the tempo track)
      return _midiFile.Chunks.OfType<TrackChunk>().Last();
   }

   bool _IsNearEvent(TimedEvent a, TimedEvent b)
   {
      return Mathf.Abs(a.Time - b.Time) < 960;
   }


   void _ParseAuthoring()
   {
      _authoring.Clear();

      if (!_HasMidiFile())
         return;

      TrackChunk authTrack = _FindAuthoringTracks();

      if (authTrack == null)
         return;

      using (TimedEventsManager timedEventsManager = authTrack.ManageTimedEvents())
      {
         // Get timed events ordered by time
         TimedEventsCollection events = timedEventsManager.Events;

         AuthoringEvent newEvent = null;
         int curNote = -1;
         foreach (var midiEvent in events)
         {
            if (!(midiEvent.Event is SequenceTrackNameEvent) && !(midiEvent.Event is BaseTextEvent) && (midiEvent.Event is NoteOnEvent)) //ignore text events!
            {
               newEvent = new AuthoringEvent();
               curNote = (midiEvent.Event as NoteOnEvent).NoteNumber;
               newEvent.NoteIdx = curNote - BaseInstrumentNote;

               if (newEvent.NoteIdx < 0) //note was less than our BaseInstrumentNote, so ignore!
                  continue;

               float startSecs = midiEvent.TimeAs<MetricTimeSpan>(_tempoMap).TotalMicroseconds / 1000000.0f;
               newEvent.NoteOnBeat = SecsToBeats(startSecs);

               _authoring.Add(newEvent);

               //now find a nearby  note-off pair
               /*foreach (var pairedEvent in events)
               {
                  if (_IsNearEvent(pairedEvent, midiEvent) && (pairedEvent.Event is NoteOffEvent) && (curNote >= 0) && (curNote == (pairedEvent.Event as NoteOffEvent).NoteNumber)) //found paired note off
                  {
                     float endSecs = pairedEvent.TimeAs<MetricTimeSpan>(_tempoMap).TotalMicroseconds / 1000000.0f;
                     newEvent.NoteOffBeat = SecsToBeats(endSecs);

                     if (newEvent.IsValid()) //done?
                     {
                        _authoring.Add(newEvent);

                        //Debug.Log("added event " + newEvent.NoteOnBeat + " -> " + newEvent.NoteOffBeat);
                     }

                     break;
                  }
               }*/
            }

         }
      }

      Debug.Log("Found " + _authoring.Count + " authoring events!");
   }

   bool _HasMidiFile()
   {
      return _tempoMap != null;
   }

   bool _LoadMidiFile(string path)
   {
      if (_HasMidiFile() && _loadedMidiPath.Equals(path)) //already loaded?
         return true;

      _tempoMap = null;

      string fullMidiPath = Application.streamingAssetsPath + "/" + path;
      _midiFile = MidiFile.Read(fullMidiPath);

      if (_midiFile == null)
      {
         Debug.LogWarning("FAILED to Load '" + fullMidiPath + "'");
         return false;
      }
      else
         Debug.Log("Loaded '" + fullMidiPath + "'");


      _tempoMap = _midiFile.GetTempoMap();

      _ParseAuthoring();

      return true;
   }

   public float GetCurContentTime()
   {
      return _source ? _source.time : 0.0f;
   }

   public float GetLengthSeconds()
   {
      return AudioToPlay ? AudioToPlay.length : 0.0f;
   }

   int _TicksPerBeat()
   {
      if (!_HasMidiFile())
         return 480;
      var ticksPerQuarterNoteTimeDivision = _tempoMap.TimeDivision as TicksPerQuarterNoteTimeDivision;
      return ticksPerQuarterNoteTimeDivision.ToInt16();
   }


   public float SecsToBeats(float secs)
   {
      if (!_HasMidiFile())
         return 0.0f;

      long microseconds = (long)(secs * 1000000.0f);

      MetricTimeSpan inputSpan = new MetricTimeSpan(microseconds);
      BeatTimeSpan outputSpan = TimeConverter.ConvertTo<BeatTimeSpan>(inputSpan, _tempoMap);


      float beats = (float)outputSpan.Beats;
      beats += ((float)outputSpan.Ticks / _TicksPerBeat());

      return beats;
   }

   public float BeatsToSecs(float beat)
   {
      if (!_HasMidiFile())
         return 0.0f;

      float frac = beat % 1.0f;
      int beats = (int)beat;
      int ticks = (int)(frac * _TicksPerBeat());
      BeatTimeSpan inputSpan = new BeatTimeSpan(beats, ticks);
      MetricTimeSpan outputSpan = TimeConverter.ConvertTo<MetricTimeSpan>(inputSpan, _tempoMap);

      float secs = outputSpan.TotalMicroseconds / 1000000.0f;

      return secs;
   }

   public Mbt BeatsToMBT(float beat)
   {
      if (!_HasMidiFile())
         return new Mbt(0, (int)beat, 0);

      float frac = beat % 1.0f;
      int beats = (int)beat;
      int ticks = (int)(frac * _TicksPerBeat());
      BeatTimeSpan inputSpan = new BeatTimeSpan(beats, ticks);

      BarBeatTimeSpan outputSpan = TimeConverter.ConvertTo<BarBeatTimeSpan>(inputSpan, _tempoMap);

      return new Mbt((int)outputSpan.Bars, (int)outputSpan.Beats, (int)outputSpan.Ticks);
   }

   public float MBTToBeats(Mbt mbt)
   {
      if (!_HasMidiFile())
         return mbt.beat * 4;

      BarBeatTimeSpan inputSpan = new BarBeatTimeSpan(mbt.measure, mbt.beat, mbt.tick);
      BeatTimeSpan outputSpan = TimeConverter.ConvertTo<BeatTimeSpan>(inputSpan, _tempoMap);

      float resultBeats = (float)outputSpan.Beats + ((float)outputSpan.Ticks / _TicksPerBeat());

      return resultBeats;
   }
}
