using System;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal sealed class BeatTimeSpanConverter : ITimeSpanConverter
    {
        #region ITimeSpanConverter

        public ITimeSpan ConvertTo(long timeSpan, long time, TempoMap tempoMap)
        {
            var ticksPerQuarterNoteTimeDivision = tempoMap.TimeDivision as TicksPerQuarterNoteTimeDivision;
            if (ticksPerQuarterNoteTimeDivision == null)
                throw new ArgumentException("Time division is not supported for time span conversion.", nameof(tempoMap));

            if (timeSpan == 0)
                return new BeatTimeSpan();

            var ticksPerQuarterNote = ticksPerQuarterNoteTimeDivision.TicksPerQuarterNote;
            var endTime = time + timeSpan;

            //

            var timeSignatureLine = tempoMap.TimeSignature;
            var timeSignatureChanges = timeSignatureLine
                .Where(v => v.Time > time && v.Time < endTime)
                .ToList();

            var bars = 0L;
            var totalBeats = 0L;

            // Calculate count of complete bars between time signature changes

            for (int i = 0; i < timeSignatureChanges.Count - 1; i++)
            {
                var timeSignatureChange = timeSignatureChanges[i];
                var nextTime = timeSignatureChanges[i + 1].Time;

                var barLength = GetBarLength(timeSignatureChange.Value, ticksPerQuarterNote);
                var numBars = (nextTime - timeSignatureChange.Time) / barLength;
                bars += numBars;

                var beatsPerBar = GetBeatsPerBar(timeSignatureChange.Value, ticksPerQuarterNote);
                totalBeats += numBars * beatsPerBar;
            }

            // Calculate components before first time signature change and after last time signature change

            var firstTime = timeSignatureChanges.FirstOrDefault()?.Time ?? time;
            var lastTime = timeSignatureChanges.LastOrDefault()?.Time ?? time;

            var firstTimeSignature = timeSignatureLine.AtTime(time);
            var lastTimeSignature = timeSignatureLine.AtTime(lastTime);

            long barsBefore, beatsBefore, ticksBefore;
            CalculateComponents(firstTime - time,
                                firstTimeSignature,
                                ticksPerQuarterNote,
                                out barsBefore,
                                out beatsBefore,
                                out ticksBefore);

            long barsAfter, beatsAfter, ticksAfter;
            CalculateComponents(time + timeSpan - lastTime,
                                lastTimeSignature,
                                ticksPerQuarterNote,
                                out barsAfter,
                                out beatsAfter,
                                out ticksAfter);

            bars += barsBefore + barsAfter;

            var beforeBeatsPerBar = GetBeatsPerBar(firstTimeSignature, ticksPerQuarterNote);
            totalBeats += barsBefore * beforeBeatsPerBar;
            var afterBeatsPerBar = GetBeatsPerBar(lastTimeSignature, ticksPerQuarterNote);
            totalBeats += barsAfter * afterBeatsPerBar;

            // Try to complete a bar

            var beats = beatsBefore + beatsAfter;
            totalBeats += beatsBefore + beatsAfter;
            if (beats > 0)
            {
                if (beatsBefore > 0 && beats >= firstTimeSignature.Numerator)
                {
                    bars++;
                    beats -= firstTimeSignature.Numerator;

                    totalBeats += beforeBeatsPerBar;
                    totalBeats -= firstTimeSignature.Numerator;
               }
            }

            // Try to complete a beat

            var ticks = ticksBefore + ticksAfter;
            if (ticks > 0)
            {
                var beatLength = GetBeatLength(firstTimeSignature, ticksPerQuarterNote);

                if (ticksBefore > 0 && ticks >= beatLength)
                {
                    beats++;
                    ticks -= beatLength;

                    totalBeats++;
                }

                //MMANDEL: we interpret ticks as going from 0 to ticksPerQuarterNote
                float tickU = ((float)ticks / beatLength);
                ticks = (int)(tickU * ticksPerQuarterNote);
         }

         return new BeatTimeSpan(totalBeats, ticks);
        }

        public long ConvertFrom(ITimeSpan timeSpan, long time, TempoMap tempoMap)
        {
            var ticksPerQuarterNoteTimeDivision = tempoMap.TimeDivision as TicksPerQuarterNoteTimeDivision;
            if (ticksPerQuarterNoteTimeDivision == null)
                throw new ArgumentException("Time division is not supported for time span conversion.", nameof(tempoMap));

            var beatTimeSpan = (BeatTimeSpan)timeSpan;
            if (beatTimeSpan.Beats == 0 && beatTimeSpan.Ticks == 0)
                return 0;

            var ticksPerQuarterNote = ticksPerQuarterNoteTimeDivision.TicksPerQuarterNote;
            var timeSignatureLine = tempoMap.TimeSignature;

            //

            long beats = beatTimeSpan.Beats;
            long ticks = beatTimeSpan.Ticks;

            var startTimeSignature = timeSignatureLine.AtTime(time);
            var startBarLength = GetBarLength(startTimeSignature, ticksPerQuarterNote);
            var startBeatsPerBar = GetBeatsPerBar(startTimeSignature, ticksPerQuarterNote);
            var startBeatLength = GetBeatLength(startTimeSignature, ticksPerQuarterNote);

            //convert ticks from quarter note space to beat length space
            float tickU = (float)ticks / ticksPerQuarterNote;
            ticks = (int)(tickU * startBeatLength);

            var totalTicks =  beats * startBeatLength + ticks;
            var timeSignatureChanges = timeSignatureLine.Where(v => v.Time > time && v.Time < time + totalTicks).ToList();

            var lastBarLength = 0L;
            var lastBeatLength = 0L;

            var firstTimeSignatureChange = timeSignatureChanges.FirstOrDefault();
            var lastTimeSignature = firstTimeSignatureChange?.Value ?? startTimeSignature;
            var lastTime = firstTimeSignatureChange?.Time ?? time;

            long barsBefore, beatsBefore, ticksBefore;
            CalculateComponents(lastTime - time,
                                startTimeSignature,
                                ticksPerQuarterNote,
                                out barsBefore,
                                out beatsBefore,
                                out ticksBefore);

            beats -= barsBefore*startBeatsPerBar;

            // Balance bars

            /*foreach (var timeSignatureChange in timeSignatureLine.Where(v => v.Time > lastTime).ToList())
            {
                var deltaTime = timeSignatureChange.Time - lastTime;

                lastBarLength = GetBarLength(lastTimeSignature, ticksPerQuarterNote);
                lastBeatLength = GetBeatLength(lastTimeSignature, ticksPerQuarterNote);

                var currentBars = Math.Min(deltaTime / lastBarLength, bars);
                bars -= currentBars;
                lastTime += currentBars * lastBarLength;

                if (bars == 0)
                    break;

                lastTimeSignature = timeSignatureChange.Value;
            }

            if (bars > 0)
            {
                lastBarLength = GetBarLength(lastTimeSignature, ticksPerQuarterNote);
                lastBeatLength = GetBeatLength(lastTimeSignature, ticksPerQuarterNote);
                lastTime += bars * lastBarLength;
            }*/

            if (beats == beatsBefore && ticks == ticksBefore)
                return lastTime - time;

            // Balance beats

            if (beatsBefore > beats && lastBarLength > 0)
            {
                lastTime += -lastBarLength + (startTimeSignature.Numerator - beatsBefore) * lastBeatLength;
                beatsBefore = 0;
            }

            if (beatsBefore < beats)
            {
                lastBeatLength = GetBeatLength(timeSignatureLine.AtTime(lastTime), ticksPerQuarterNote);
                lastTime += (beats - beatsBefore) * lastBeatLength;
            }

            // Balance ticks

            if (ticksBefore > ticks && lastBeatLength > 0)
            {
                lastTime += -lastBeatLength + startBeatLength - ticksBefore;
                ticksBefore = 0;
            }

            if (ticksBefore < ticks)
                lastTime += ticks - ticksBefore;

            //

            return lastTime - time;
        }

        #endregion

        #region Methods

        private static void CalculateComponents(long totalTicks,
                                                TimeSignature timeSignature,
                                                short ticksPerQuarterNote,
                                                out long bars,
                                                out long beats,
                                                out long ticks)
        {
            var barLength = GetBarLength(timeSignature, ticksPerQuarterNote);
            bars = Math.DivRem(totalTicks, barLength, out ticks);

            var beatLength = GetBeatLength(timeSignature, ticksPerQuarterNote);
            beats = Math.DivRem(ticks, beatLength, out ticks);
        }

        private static int GetBarLength(TimeSignature timeSignature, short ticksPerQuarterNote)
        {
            var beatLength = GetBeatLength(timeSignature, ticksPerQuarterNote);
            return timeSignature.Numerator * beatLength;
        }

        private static int GetBeatLength(TimeSignature timeSignature, short ticksPerQuarterNote)
        {
            return 4 * ticksPerQuarterNote / timeSignature.Denominator;
        }

      private static int GetBeatsPerBar(TimeSignature timeSignature, short ticksPerQuarterNote)
      {
         return GetBarLength(timeSignature, ticksPerQuarterNote) / GetBeatLength(timeSignature, ticksPerQuarterNote);
      }

      #endregion
   }
}
