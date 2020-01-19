﻿using Melanchall.DryWetMidi.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    /// <summary>
    /// Represents a tempo map of a MIDI file.
    /// </summary>
    public sealed class TempoMap
    {
        #region Constants

        /// <summary>
        /// The default tempo map which uses 4/4 time signature and tempo of 500,000 microseconds per quarter note.
        /// </summary>
        public static readonly TempoMap Default = new TempoMap(new TicksPerQuarterNoteTimeDivision());

        #endregion

        #region Fields

        private ValueLine<TimeSignature> _timeSignature;
        private ValueLine<Tempo> _tempo;

        private readonly List<ITempoMapValuesCache> _valuesCaches = new List<ITempoMapValuesCache>();

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TempoMap"/> with the specified time division
        /// of a MIDI file.
        /// </summary>
        /// <param name="timeDivision">MIDI file time division which specifies the meaning of the time
        /// used by events of the file.</param>
        /// <exception cref="ArgumentNullException"><paramref name="timeDivision"/> is null.</exception>
        internal TempoMap(TimeDivision timeDivision)
        {
            ThrowIfArgument.IsNull(nameof(timeDivision), timeDivision);

            TimeDivision = timeDivision;
            Tempo = new ValueLine<Tempo>(Interaction.Tempo.Default);
            TimeSignature = new ValueLine<TimeSignature>(Interaction.TimeSignature.Default);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the time division used by a tempo map.
        /// </summary>
        public TimeDivision TimeDivision { get; internal set; }

        /// <summary>
        /// Gets an object that holds changes of the time signature through the time.
        /// </summary>
        public ValueLine<TimeSignature> TimeSignature
        {
            get { return _timeSignature; }
            private set
            {
                if (_timeSignature != null)
                    _timeSignature.ValuesChanged -= OnTimeSignatureChanged;

                _timeSignature = value;
                _timeSignature.ValuesChanged += OnTimeSignatureChanged;
            }
        }

        /// <summary>
        /// Gets an object that holds changes of the tempo through the time.
        /// </summary>
        public ValueLine<Tempo> Tempo
        {
            get { return _tempo; }
            private set
            {
                if (_tempo != null)
                    _tempo.ValuesChanged -= OnTempoChanged;

                _tempo = value;
                _tempo.ValuesChanged += OnTempoChanged;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Clones the current <see cref="TempoMap"/>.
        /// </summary>
        /// <returns>An instance of the <see cref="TempoMap"/> which is a clone of the current one.</returns>
        public TempoMap Clone()
        {
            var tempoMap = new TempoMap(TimeDivision.Clone());

            tempoMap.Tempo.ReplaceValues(Tempo);
            tempoMap.TimeSignature.ReplaceValues(TimeSignature);

            return tempoMap;
        }

        /// <summary>
        /// Creates an instance of the <see cref="TempoMap"/> with the specified tempo and
        /// time signature using default time division (96 ticks per quarter note).
        /// </summary>
        /// <param name="tempo">Tempo of the tempo map.</param>
        /// <param name="timeSignature">Time signature of the tempo map.</param>
        /// <returns><see cref="TempoMap"/> with the specified tempo and time signature.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tempo"/> is null. -or-
        /// <paramref name="timeSignature"/> is null.</exception>
        public static TempoMap Create(Tempo tempo, TimeSignature timeSignature)
        {
            ThrowIfArgument.IsNull(nameof(tempo), tempo);
            ThrowIfArgument.IsNull(nameof(timeSignature), timeSignature);

            var tempoMap = Default.Clone();
            SetGlobalTempo(tempoMap, tempo);
            SetGlobalTimeSignature(tempoMap, timeSignature);

            return tempoMap;
        }

        /// <summary>
        /// Creates an instance of the <see cref="TempoMap"/> with the specified tempo using
        /// default time division (96 ticks per quarter note).
        /// </summary>
        /// <param name="tempo">Tempo of the tempo map.</param>
        /// <returns><see cref="TempoMap"/> with the specified tempo.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tempo"/> is null.</exception>
        public static TempoMap Create(Tempo tempo)
        {
            ThrowIfArgument.IsNull(nameof(tempo), tempo);

            var tempoMap = Default.Clone();
            SetGlobalTempo(tempoMap, tempo);

            return tempoMap;
        }

        public static TempoMap Create(TimeSignature timeSignature)
        {
            ThrowIfArgument.IsNull(nameof(timeSignature), timeSignature);

            var tempoMap = Default.Clone();
            SetGlobalTimeSignature(tempoMap, timeSignature);

            return tempoMap;
        }

        public static TempoMap Create(TimeDivision timeDivision)
        {
            ThrowIfArgument.IsNull(nameof(timeDivision), timeDivision);

            return new TempoMap(timeDivision);
        }

        public static TempoMap Create(TimeDivision timeDivision, Tempo tempo)
        {
            ThrowIfArgument.IsNull(nameof(timeDivision), timeDivision);
            ThrowIfArgument.IsNull(nameof(tempo), tempo);

            var tempoMap = new TempoMap(timeDivision);
            SetGlobalTempo(tempoMap, tempo);

            return tempoMap;
        }

        public static TempoMap Create(TimeDivision timeDivision, TimeSignature timeSignature)
        {
            ThrowIfArgument.IsNull(nameof(timeDivision), timeDivision);
            ThrowIfArgument.IsNull(nameof(timeSignature), timeSignature);

            var tempoMap = new TempoMap(timeDivision);
            SetGlobalTimeSignature(tempoMap, timeSignature);

            return tempoMap;
        }

        public static TempoMap Create(TimeDivision timeDivision, Tempo tempo, TimeSignature timeSignature)
        {
            ThrowIfArgument.IsNull(nameof(timeDivision), timeDivision);
            ThrowIfArgument.IsNull(nameof(tempo), tempo);
            ThrowIfArgument.IsNull(nameof(timeSignature), timeSignature);

            var tempoMap = new TempoMap(timeDivision);
            SetGlobalTempo(tempoMap, tempo);
            SetGlobalTimeSignature(tempoMap, timeSignature);

            return tempoMap;
        }

        /// <summary>
        /// Flips the tempo map relative to the specified time.
        /// </summary>
        /// <param name="centerTime">The time the tempo map should be flipped relative to.</param>
        /// <returns>The tempo mup flipped relative to the <paramref name="centerTime"/>.</returns>
        internal TempoMap Flip(long centerTime)
        {
            return new TempoMap(TimeDivision)
            {
                Tempo = Tempo.Reverse(centerTime),
                TimeSignature = TimeSignature.Reverse(centerTime)
            };
        }

        internal TCache GetValuesCache<TCache>() where TCache : ITempoMapValuesCache, new()
        {
            var result = _valuesCaches.OfType<TCache>().FirstOrDefault();
            if (result == null)
            {
                _valuesCaches.Add(result = new TCache());
                result.Invalidate(this);
            }

            return result;
        }

        private static void SetGlobalTempo(TempoMap tempoMap, Tempo tempo)
        {
            tempoMap.Tempo.SetValue(0, tempo);
        }

        private static void SetGlobalTimeSignature(TempoMap tempoMap, TimeSignature timeSignature)
        {
            tempoMap.TimeSignature.SetValue(0, timeSignature);
        }

        private void InvalidateCaches(TempoMapLine tempoMapLine)
        {
            foreach (var valuesCache in _valuesCaches.Where(c => c.InvalidateOnLines?.Contains(tempoMapLine) == true))
            {
                valuesCache.Invalidate(this);
            }
        }

        private void OnTimeSignatureChanged(object sender, EventArgs args)
        {
            InvalidateCaches(TempoMapLine.TimeSignature);
        }

        private void OnTempoChanged(object sender, EventArgs args)
        {
            InvalidateCaches(TempoMapLine.Tempo);
        }

        #endregion
    }
}
