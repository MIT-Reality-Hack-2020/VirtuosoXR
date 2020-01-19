﻿using System.ComponentModel;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;

namespace Melanchall.DryWetMidi.Standards
{
    /// <summary>
    /// Provides utilities for General MIDI Level 1.
    /// </summary>
    public static class GeneralMidiUtilities
    {
        #region Methods

        /// <summary>
        /// Converts <see cref="GeneralMidiProgram"/> to the corresponding value of the
        /// <see cref="SevenBitNumber"/> type.
        /// </summary>
        /// <param name="program"><see cref="GeneralMidiProgram"/> to convert to <see cref="SevenBitNumber"/>.</param>
        /// <returns><see cref="SevenBitNumber"/> representing the <paramref name="program"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="program"/> specified an invalid value.</exception>
        public static SevenBitNumber AsSevenBitNumber(this GeneralMidiProgram program)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(program), program);

            return (SevenBitNumber)(byte)program;
        }

        /// <summary>
        /// Converts <see cref="GeneralMidiPercussion"/> to the corresponding value of the
        /// <see cref="SevenBitNumber"/> type.
        /// </summary>
        /// <param name="percussion"><see cref="GeneralMidiPercussion"/> to convert to <see cref="SevenBitNumber"/>.</param>
        /// <returns><see cref="SevenBitNumber"/> representing the <paramref name="percussion"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussion"/> specified an invalid value.</exception>
        public static SevenBitNumber AsSevenBitNumber(this GeneralMidiPercussion percussion)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return (SevenBitNumber)(byte)percussion;
        }

        /// <summary>
        /// Gets an instance of the <see cref="ProgramChangeEvent"/> corresponding to the specified
        /// General MIDI Level 1 program.
        /// </summary>
        /// <param name="program"><see cref="GeneralMidiProgram"/> to get an event for.</param>
        /// <returns>An instance of the <see cref="ProgramChangeEvent"/> corresponding to the <paramref name="program"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="program"/> specified an invalid value.</exception>
        public static MidiEvent GetProgramEvent(this GeneralMidiProgram program)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(program), program);

            return new ProgramChangeEvent(program.AsSevenBitNumber());
        }

        /// <summary>
        /// Gets an instance of the <see cref="NoteOnEvent"/> corresponding to the specified
        /// General MIDI Level 1 percussion.
        /// </summary>
        /// <param name="percussion"><see cref="GeneralMidiPercussion"/> to get an event for.</param>
        /// <param name="velocity">Velocity of the <see cref="NoteOnEvent"/>.</param>
        /// <returns>An instance of the <see cref="NoteOnEvent"/> corresponding to the specified
        /// <paramref name="percussion"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussion"/> specified an invalid value.</exception>
        public static NoteOnEvent GetNoteOnEvent(this GeneralMidiPercussion percussion, SevenBitNumber velocity)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return new NoteOnEvent(percussion.AsSevenBitNumber(), velocity);
        }

        /// <summary>
        /// Gets an instance of the <see cref="NoteOffEvent"/> corresponding to the specified
        /// General MIDI Level 1 percussion.
        /// </summary>
        /// <param name="percussion"><see cref="GeneralMidiPercussion"/> to get an event for.</param>
        /// <param name="velocity">Velocity of the <see cref="NoteOffEvent"/>.</param>
        /// <returns>An instance of the <see cref="NoteOffEvent"/> corresponding to the specified
        /// <paramref name="percussion"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussion"/> specified an invalid value.</exception>
        public static NoteOffEvent GetNoteOffEvent(this GeneralMidiPercussion percussion, SevenBitNumber velocity)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return new NoteOffEvent(percussion.AsSevenBitNumber(), velocity);
        }

        #endregion
    }
}
