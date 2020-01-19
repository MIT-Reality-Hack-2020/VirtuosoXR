﻿using Melanchall.DryWetMidi.Common;
using System;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Represents a MIDI file system exclusive event.
    /// </summary>
    /// <remarks>
    /// System exclusive events are used to specify a MIDI system exclusive message, either as one unit or in packets,
    /// or as an "escape" to specify any arbitrary bytes to be transmitted.
    /// </remarks>
    public abstract class SysExEvent : MidiEvent
    {
        #region Constants

        private const byte EndOfEventByte = 0xF7;

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether this sytem exclusive event is completed or not.
        /// </summary>
        public bool Completed => Data?.LastOrDefault() == EndOfEventByte;

        /// <summary>
        /// Gets or sets the event's data.
        /// </summary>
        public byte[] Data { get; set; }

        #endregion

        #region Overrides

        /// <summary>
        /// Reads content of a MIDI event.
        /// </summary>
        /// <param name="reader">Reader to read the content with.</param>
        /// <param name="settings">Settings according to which the event's content must be read.</param>
        /// <param name="size">Size of the event's content.</param>
        internal sealed override void Read(MidiReader reader, ReadingSettings settings, int size)
        {
            ThrowIfArgument.IsNegative(nameof(size),
                                        size,
                                        "Non-negative size have to be specified in order to read SysEx event.");

            Data = reader.ReadBytes(size);
        }

        /// <summary>
        /// Writes content of a MIDI event.
        /// </summary>
        /// <param name="writer">Writer to write the content with.</param>
        /// <param name="settings">Settings according to which the event's content must be written.</param>
        internal sealed override void Write(MidiWriter writer, WritingSettings settings)
        {
            var data = Data;
            if (data != null)
                writer.WriteBytes(data);
        }

        /// <summary>
        /// Gets the size of the content of a MIDI event.
        /// </summary>
        /// <param name="settings">Settings according to which the event's content must be written.</param>
        /// <returns>Size of the event's content.</returns>
        internal sealed override int GetSize(WritingSettings settings)
        {
            return Data?.Length ?? 0;
        }

        /// <summary>
        /// Clones event by creating a copy of it.
        /// </summary>
        /// <returns>Copy of the event.</returns>
        protected sealed override MidiEvent CloneEvent()
        {
            var eventType = GetType();
            var sysExEvent = (SysExEvent)Activator.CreateInstance(eventType);

            sysExEvent.Data = Data?.Clone() as byte[];

            return sysExEvent;
        }

        #endregion
    }
}
