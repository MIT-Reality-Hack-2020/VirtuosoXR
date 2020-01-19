﻿using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Grid where points in time are distributed arbitrarily according to provided collection
    /// of times.
    /// </summary>
    public sealed class ArbitraryGrid : IGrid
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ArbitraryGrid"/> with the specified
        /// collection of times.
        /// </summary>
        /// <param name="times">Grid's times.</param>
        /// <exception cref="ArgumentNullException"><paramref name="times"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="times"/> contains null.</exception>
        public ArbitraryGrid(IEnumerable<ITimeSpan> times)
        {
            ThrowIfArgument.IsNull(nameof(times), times);
            ThrowIfArgument.ContainsNull(nameof(times), times);

            Times = times;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Times of the current grid.
        /// </summary>
        public IEnumerable<ITimeSpan> Times { get; }

        #endregion

        #region IGrid

        /// <summary>
        /// Gets points in time of the current grid.
        /// </summary>
        /// <param name="tempoMap">Tempo map used to get grid's times.</param>
        /// <returns>Collection of points in time of the current grid.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tempoMap"/> is null.</exception>
        public IEnumerable<long> GetTimes(TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            return Times.Select(t => TimeConverter.ConvertFrom(t, tempoMap));
        }

        #endregion
    }
}
