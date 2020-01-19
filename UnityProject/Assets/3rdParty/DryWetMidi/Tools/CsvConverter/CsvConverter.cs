﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Provides methods to convert MIDI objects to CSV representation and vice versa.
    /// </summary>
    public sealed class CsvConverter
    {
        #region Methods

        /// <summary>
        /// Converts the specified <see cref="MidiFile"/> to CSV represenattion and writes it to a file.
        /// </summary>
        /// <remarks>
        /// Note that <see cref="MidiFile"/> can be converted to different CSV representations. You can specify desired
        /// CSV layout via <paramref name="settings"/> using <see cref="MidiFileCsvConversionSettings.CsvLayout"/> property.
        /// </remarks>
        /// <param name="midiFile"><see cref="MidiFile"/> to convert to CSV.</param>
        /// <param name="filePath">Path of the output CSV file.</param>
        /// <param name="overwriteFile">If true and file specified by <paramref name="filePath"/> already
        /// exists it will be overwritten; if false and the file exists exception will be thrown.</param>
        /// <param name="settings">Settings according to which <paramref name="midiFile"/> must be converted.</param>
        /// <exception cref="ArgumentNullException"><paramref name="midiFile"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="filePath"/> is a zero-length string,
        /// contains only white space, or contains one or more invalid characters as defined by
        /// <see cref="Path.InvalidPathChars"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="filePath"/> is null.</exception>
        /// <exception cref="PathTooLongException">The specified path, file name, or both exceed the system-defined
        /// maximum length. For example, on Windows-based platforms, paths must be less than 248 characters,
        /// and file names must be less than 260 characters.</exception>
        /// <exception cref="DirectoryNotFoundException">The specified path is invalid, (for example,
        /// it is on an unmapped drive).</exception>
        /// <exception cref="IOException">An I/O error occurred while writing the file.</exception>
        /// <exception cref="NotSupportedException"><paramref name="filePath"/> is in an invalid format.</exception>
        /// <exception cref="UnauthorizedAccessException">This operation is not supported on the current platform.-or-
        /// <paramref name="filePath"/> specified a directory.-or- The caller does not have the required permission.</exception>
        public void ConvertMidiFileToCsv(MidiFile midiFile, string filePath, bool overwriteFile = false, MidiFileCsvConversionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);

            using (var fileStream = FileUtilities.OpenFileForWrite(filePath, overwriteFile))
            {
                ConvertMidiFileToCsv(midiFile, fileStream, settings);
            }
        }

        /// <summary>
        /// Converts the specified <see cref="MidiFile"/> to CSV represenattion and writes it to a stream.
        /// </summary>
        /// <remarks>
        /// Note that <see cref="MidiFile"/> can be converted to different CSV representations. You can specify desired
        /// CSV layout via <paramref name="settings"/> using <see cref="MidiFileCsvConversionSettings.CsvLayout"/> property.
        /// </remarks>
        /// <param name="midiFile"><see cref="MidiFile"/> to convert to CSV.</param>
        /// <param name="stream">Stream to write CSV representation to.</param>
        /// <param name="settings">Settings according to which <paramref name="midiFile"/> must be converted.</param>
        /// <exception cref="ArgumentNullException"><paramref name="midiFile"/> is null. -or-
        /// <paramref name="stream"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="stream"/> doesn't support writing.</exception>
        /// <exception cref="IOException">An I/O error occurred while writing to the stream.</exception>
        /// <exception cref="ObjectDisposedException"><paramref name="stream"/> is disposed.</exception>
        public void ConvertMidiFileToCsv(MidiFile midiFile, Stream stream, MidiFileCsvConversionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(stream), stream);

            if (!stream.CanWrite)
                throw new ArgumentException("Stream doesn't support writing.", nameof(stream));

            MidiFileToCsvConverter.ConvertToCsv(midiFile, stream, settings ?? new MidiFileCsvConversionSettings());
        }

        /// <summary>
        /// Converts CSV representation of a MIDI file to <see cref="MidiFile"/> reading CSV data from a file.
        /// </summary>
        /// <remarks>
        /// Note that CSV representation of a MIDI file can be different. You can specify expected CSV layout
        /// via <paramref name="settings"/> using <see cref="MidiFileCsvConversionSettings.CsvLayout"/> property.
        /// </remarks>
        /// <param name="filePath">Path of the file with CSV representation of a MIDI file.</param>
        /// <param name="settings">Settings according to which CSV data must be converted.</param>
        /// <returns>An instance of the <see cref="MidiFile"/> representing a MIDI file written in CSV format.</returns>
        /// <exception cref="ArgumentException"><paramref name="filePath"/> is a zero-length string,
        /// contains only white space, or contains one or more invalid characters as defined by
        /// <see cref="Path.InvalidPathChars"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="filePath"/> is null.</exception>
        /// <exception cref="PathTooLongException">The specified path, file name, or both exceed the system-defined
        /// maximum length. For example, on Windows-based platforms, paths must be less than 248 characters,
        /// and file names must be less than 260 characters.</exception>
        /// <exception cref="DirectoryNotFoundException">The specified path is invalid, (for example,
        /// it is on an unmapped drive).</exception>
        /// <exception cref="IOException">An I/O error occurred while reading the file.</exception>
        /// <exception cref="NotSupportedException"><paramref name="filePath"/> is in an invalid format.</exception>
        /// <exception cref="UnauthorizedAccessException">This operation is not supported on the current platform. -or-
        /// <paramref name="filePath"/> specified a directory. -or- The caller does not have the required permission.</exception>
        public MidiFile ConvertCsvToMidiFile(string filePath, MidiFileCsvConversionSettings settings = null)
        {
            using (var fileStream = FileUtilities.OpenFileForRead(filePath))
            {
                return ConvertCsvToMidiFile(fileStream, settings);
            }
        }

        /// <summary>
        /// Converts CSV representation of a MIDI file to <see cref="MidiFile"/> readong CSV data from a stream.
        /// </summary>
        /// <param name="stream">Stream to read MIDI file from.</param>
        /// <param name="settings">Settings according to which CSV data must be converted.</param>
        /// <returns>An instance of the <see cref="MidiFile"/> representing a MIDI file written in CSV format.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="stream"/> doesn't support reading.</exception>
        /// <exception cref="IOException">An I/O error occurred while reading from the stream.</exception>
        /// <exception cref="ObjectDisposedException"><paramref name="stream"/> is disposed.</exception>
        public MidiFile ConvertCsvToMidiFile(Stream stream, MidiFileCsvConversionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(stream), stream);

            if (!stream.CanRead)
                throw new ArgumentException("Stream doesn't support reading.", nameof(stream));

            return CsvToMidiFileConverter.ConvertToMidiFile(stream, settings ?? new MidiFileCsvConversionSettings());
        }

        public void ConvertNotesToCsv(IEnumerable<Note> notes, string filePath, TempoMap tempoMap, bool overwriteFile = false, NoteCsvConversionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(notes), notes);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            using (var fileStream = FileUtilities.OpenFileForWrite(filePath, overwriteFile))
            {
                ConvertNotesToCsv(notes, fileStream, tempoMap, settings);
            }
        }

        public void ConvertNotesToCsv(IEnumerable<Note> notes, Stream stream, TempoMap tempoMap, NoteCsvConversionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(notes), notes);
            ThrowIfArgument.IsNull(nameof(stream), stream);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            if (!stream.CanWrite)
                throw new ArgumentException("Stream doesn't support writing.", nameof(stream));

            NotesToCsvConverter.ConvertToCsv(notes, stream, tempoMap, settings ?? new NoteCsvConversionSettings());
        }

        public IEnumerable<Note> ConvertCsvToNotes(string filePath, TempoMap tempoMap, NoteCsvConversionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            using (var fileStream = FileUtilities.OpenFileForRead(filePath))
            {
                return ConvertCsvToNotes(fileStream, tempoMap, settings).ToList();
            }
        }

        public IEnumerable<Note> ConvertCsvToNotes(Stream stream, TempoMap tempoMap, NoteCsvConversionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(stream), stream);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            if (!stream.CanRead)
                throw new ArgumentException("Stream doesn't support reading.", nameof(stream));

            return CsvToNotesConverter.ConvertToNotes(stream, tempoMap, settings ?? new NoteCsvConversionSettings());
        }

        #endregion
    }
}
