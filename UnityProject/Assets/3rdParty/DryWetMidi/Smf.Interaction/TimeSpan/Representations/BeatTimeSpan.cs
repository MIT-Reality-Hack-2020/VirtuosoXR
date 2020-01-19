using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
   /// <summary>
   /// Represents beat time span which represents total beats and ticks.
   /// </summary>
   public sealed class BeatTimeSpan : ITimeSpan, IComparable<BeatTimeSpan>, IEquatable<BeatTimeSpan>
   {
      #region Constructor

      /// <summary>
      /// Initializes a new instance of the <see cref="BeatTimeSpan"/>.
      /// </summary>
      public BeatTimeSpan()
          : this(0, 0)
      {
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="BeatTimeSpan"/> with the specified
      /// number of beats.
      /// </summary>
      /// <param name="beats">The number of beats.</param>
      ///       /// <param name="ticks">The number of ticks.</param>
      /// <exception cref="ArgumentOutOfRangeException"><paramref name="ticks"/> is negative. -or-
      /// <paramref name="beats"/> is negative.</exception>
      public BeatTimeSpan(long beats, long ticks)
      {
         ThrowIfArgument.IsNegative(nameof(beats), beats, "Beats number is negative.");
         ThrowIfArgument.IsNegative(nameof(ticks), ticks, "Ticks number is negative.");

         Beats = beats;
         Ticks = ticks;
      }


      #endregion

      #region Properties

      /// Gets the beats component of the time represented by the current <see cref="BeatTimeSpan"/>.
      /// </summary>
      public long Beats { get; }

      /// <summary>
      /// Gets the ticks component of the time represented by the current <see cref="BeatTimeSpan"/>.
      /// </summary>
      public long Ticks { get; }

      #endregion

      #region Methods

      /// <summary>
      /// Converts the string representation of a beat time span to its <see cref="BeatTimeSpan"/>
      /// equivalent. A return value indicates whether the conversion succeeded.
      /// </summary>
      /// <param name="input">A string containing a time span to convert.</param>
      /// <param name="timeSpan">When this method returns, contains the <see cref="BeatTimeSpan"/>
      /// equivalent of the time span contained in <paramref name="input"/>, if the conversion succeeded, or
      /// null if the conversion failed. The conversion fails if the <paramref name="input"/> is null or
      /// <see cref="String.Empty"/>, or is not of the correct format. This parameter is passed uninitialized;
      /// any value originally supplied in result will be overwritten.</param>
      /// <returns>true if <paramref name="input"/> was converted successfully; otherwise, false.</returns>
      public static bool TryParse(string input, out BeatTimeSpan timeSpan)
      {
         return BeatTimeSpanParser.TryParse(input, out timeSpan).Status == ParsingStatus.Parsed;
      }

      /// <summary>
      /// Converts the string representation of a bar/beat time span to its <see cref="BeatTimeSpan"/>
      /// equivalent.
      /// </summary>
      /// <param name="input">A string containing a time span to convert.</param>
      /// <returns>A <see cref="BeatTimeSpan"/> equivalent to the time span contained in
      /// <paramref name="input"/>.</returns>
      /// <exception cref="ArgumentException"><paramref name="input"/> is null or contains white-spaces only.</exception>
      /// <exception cref="FormatException"><paramref name="input"/> has invalid format.</exception>
      public static BeatTimeSpan Parse(string input)
      {
         BeatTimeSpan timeSpan;
         var parsingResult = BeatTimeSpanParser.TryParse(input, out timeSpan);
         if (parsingResult.Status == ParsingStatus.Parsed)
            return timeSpan;

         throw parsingResult.Exception;
      }

      #endregion

      #region Operators

      /// <summary>
      /// Determines if two <see cref="BeatTimeSpan"/> objects are equal.
      /// </summary>
      /// <param name="timeSpan1">The first <see cref="BeatTimeSpan"/> to compare.</param>
      /// <param name="timeSpan2">The second <see cref="BeatTimeSpan"/> to compare.</param>
      /// <returns>true if time spans are equal, false otherwise.</returns>
      public static bool operator ==(BeatTimeSpan timeSpan1, BeatTimeSpan timeSpan2)
      {
         if (ReferenceEquals(timeSpan1, null))
            return ReferenceEquals(timeSpan2, null);

         return timeSpan1.Equals(timeSpan2);
      }

      /// <summary>
      /// Determines if two <see cref="BeatTimeSpan"/> objects are not equal.
      /// </summary>
      /// <param name="timeSpan1">The first <see cref="BeatTimeSpan"/> to compare.</param>
      /// <param name="timeSpan2">The second <see cref="BeatTimeSpan"/> to compare.</param>
      /// <returns>false if time spans are equal, true otherwise.</returns>
      public static bool operator !=(BeatTimeSpan timeSpan1, BeatTimeSpan timeSpan2)
      {
         return !(timeSpan1 == timeSpan2);
      }

      /// <summary>
      /// Adds two specified <see cref="BeatTimeSpan"/> instances.
      /// </summary>
      /// <param name="timeSpan1">The first <see cref="BeatTimeSpan"/> to add.</param>
      /// <param name="timeSpan2">The second <see cref="BeatTimeSpan"/> to add.</param>
      /// <returns>An object whose value is the sum of the values of <paramref name="timeSpan1"/> and
      /// <paramref name="timeSpan2"/>.</returns>
      /// <exception cref="ArgumentNullException"><paramref name="timeSpan1"/> is null. -or-
      /// <paramref name="timeSpan2"/> is null.</exception>
      public static BeatTimeSpan operator +(BeatTimeSpan timeSpan1, BeatTimeSpan timeSpan2)
      {
         ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
         ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

         return new BeatTimeSpan(timeSpan1.Beats + timeSpan2.Beats,
                                    timeSpan1.Ticks + timeSpan2.Ticks);
      }

      /// <summary>
      /// Subtracts a specified <see cref="BeatTimeSpan"/> from another one.
      /// </summary>
      /// <param name="timeSpan1">The minuend.</param>
      /// <param name="timeSpan2">The subtrahend.</param>
      /// <returns>An object whose value is the result of the value of <paramref name="timeSpan1"/> minus
      /// the value of <paramref name="timeSpan2"/>.</returns>
      /// <exception cref="ArgumentNullException"><paramref name="timeSpan1"/> is null. -or-
      /// <paramref name="timeSpan2"/> is null.</exception>
      /// <exception cref="ArgumentException"><paramref name="timeSpan1"/> is less than <paramref name="timeSpan2"/>.</exception>
      public static BeatTimeSpan operator -(BeatTimeSpan timeSpan1, BeatTimeSpan timeSpan2)
      {
         ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
         ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

         if (timeSpan1 < timeSpan2)
            throw new ArgumentException("First time span is less than second one.", nameof(timeSpan1));

         return new BeatTimeSpan(timeSpan1.Beats - timeSpan2.Beats,
                                    timeSpan1.Ticks - timeSpan2.Ticks);
      }

      /// <summary>
      /// Indicates whether a specified <see cref="BeatTimeSpan"/> is less than another one.
      /// </summary>
      /// <param name="timeSpan1">The first <see cref="BeatTimeSpan"/> to compare.</param>
      /// <param name="timeSpan2">The second <see cref="BeatTimeSpan"/> to compare.</param>
      /// <returns>true if the value of <paramref name="timeSpan1"/> is less than the value of
      /// <paramref name="timeSpan2"/>; otherwise, false.</returns>
      /// <exception cref="ArgumentNullException"><paramref name="timeSpan1"/> is null. -or-
      /// <paramref name="timeSpan2"/> is null.</exception>
      public static bool operator <(BeatTimeSpan timeSpan1, BeatTimeSpan timeSpan2)
      {
         ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
         ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

         return timeSpan1.CompareTo(timeSpan2) < 0;
      }

      /// <summary>
      /// Indicates whether a specified <see cref="BeatTimeSpan"/> is greater than another one.
      /// </summary>
      /// <param name="timeSpan1">The first <see cref="BeatTimeSpan"/> to compare.</param>
      /// <param name="timeSpan2">The second <see cref="BeatTimeSpan"/> to compare.</param>
      /// <returns>true if the value of <paramref name="timeSpan1"/> is greater than the value of
      /// <paramref name="timeSpan2"/>; otherwise, false.</returns>
      /// <exception cref="ArgumentNullException"><paramref name="timeSpan1"/> is null. -or-
      /// <paramref name="timeSpan2"/> is null.</exception>
      public static bool operator >(BeatTimeSpan timeSpan1, BeatTimeSpan timeSpan2)
      {
         ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
         ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

         return timeSpan1.CompareTo(timeSpan2) > 0;
      }

      /// <summary>
      /// Indicates whether a specified <see cref="BeatTimeSpan"/> is less than or equal to
      /// another one.
      /// </summary>
      /// <param name="timeSpan1">The first <see cref="BeatTimeSpan"/> to compare.</param>
      /// <param name="timeSpan2">The second <see cref="BeatTimeSpan"/> to compare.</param>
      /// <returns>true if the value of <paramref name="timeSpan1"/> is less than or equal to the value of
      /// <paramref name="timeSpan2"/>; otherwise, false.</returns>
      /// <exception cref="ArgumentNullException"><paramref name="timeSpan1"/> is null. -or-
      /// <paramref name="timeSpan2"/> is null.</exception>
      public static bool operator <=(BeatTimeSpan timeSpan1, BeatTimeSpan timeSpan2)
      {
         ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
         ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

         return timeSpan1.CompareTo(timeSpan2) <= 0;
      }

      /// <summary>
      /// Indicates whether a specified <see cref="BeatTimeSpan"/> is greater than or equal to
      /// another one.
      /// </summary>
      /// <param name="timeSpan1">The first <see cref="BeatTimeSpan"/> to compare.</param>
      /// <param name="timeSpan2">The second <see cref="BeatTimeSpan"/> to compare.</param>
      /// <returns>true if the value of <paramref name="timeSpan1"/> is greater than or equal to the value of
      /// <paramref name="timeSpan2"/>; otherwise, false.</returns>
      /// <exception cref="ArgumentNullException"><paramref name="timeSpan1"/> is null. -or-
      /// <paramref name="timeSpan2"/> is null.</exception>
      public static bool operator >=(BeatTimeSpan timeSpan1, BeatTimeSpan timeSpan2)
      {
         ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
         ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

         return timeSpan1.CompareTo(timeSpan2) >= 0;
      }

      #endregion

      #region Overrides

      /// <summary>
      /// Determines whether the specified object is equal to the current object.
      /// </summary>
      /// <param name="obj">The object to compare with the current object.</param>
      /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
      public override bool Equals(object obj)
      {
         return Equals(obj as BeatTimeSpan);
      }

      /// <summary>
      /// Returns the hash code for this instance.
      /// </summary>
      /// <returns>A 32-bit signed integer hash code.</returns>
      public override int GetHashCode()
      {
         return Beats.GetHashCode() ^ Ticks.GetHashCode();
      }

      /// <summary>
      /// Returns a string that represents the current object.
      /// </summary>
      /// <returns>A string that represents the current object.</returns>
      public override string ToString()
      {
         return $"{Beats}.{Ticks}";
      }

      #endregion

      #region ITimeSpan

      /// <summary>
      /// Adds a time span to the current one.
      /// </summary>
      /// <remarks>
      /// If <paramref name="timeSpan"/> and the current time span have the same type,
      /// the result time span will be of this type too; otherwise - of the <see cref="MathTimeSpan"/>.
      /// </remarks>
      /// <param name="timeSpan">Time span to add to the current one.</param>
      /// <param name="mode">Mode of the operation that defines meaning of time spans the
      /// operation will be performed on.</param>
      /// <returns>Time span that is a sum of the <paramref name="timeSpan"/> and the
      /// current time span.</returns>
      /// <exception cref="ArgumentException"><paramref name="mode"/> is invalid.</exception>
      public ITimeSpan Add(ITimeSpan timeSpan, TimeSpanMode mode)
      {
         ThrowIfArgument.IsNull(nameof(timeSpan), timeSpan);

         var beatTimeSpan = timeSpan as BeatTimeSpan;
         return beatTimeSpan != null
             ? this + beatTimeSpan
             : TimeSpanUtilities.Add(this, timeSpan, mode);
      }

      /// <summary>
      /// Subtracts a time span from the current one.
      /// </summary>
      /// <remarks>
      /// If <paramref name="timeSpan"/> and the current time span have the same type,
      /// the result time span will be of this type too; otherwise - of the <see cref="MathTimeSpan"/>.
      /// </remarks>
      /// <param name="timeSpan">Time span to subtract from the current one.</param>
      /// <param name="mode">Mode of the operation that defines meaning of time spans the
      /// operation will be performed on.</param>
      /// <returns>Time span that is a difference between the <paramref name="timeSpan"/> and the
      /// current time span.</returns>
      public ITimeSpan Subtract(ITimeSpan timeSpan, TimeSpanMode mode)
      {
         ThrowIfArgument.IsNull(nameof(timeSpan), timeSpan);

         var beatTimeSpan = timeSpan as BeatTimeSpan;
         return beatTimeSpan != null
             ? this - beatTimeSpan
             : TimeSpanUtilities.Subtract(this, timeSpan, mode);
      }

      /// <summary>
      /// Stretches the current time span by multiplying its length by the specified multiplier.
      /// </summary>
      /// <param name="multiplier">Multiplier to stretch the time span by.</param>
      /// <returns>Time span that is the current time span stretched by the <paramref name="multiplier"/>.</returns>
      public ITimeSpan Multiply(double multiplier)
      {
         ThrowIfArgument.IsNegative(nameof(multiplier), multiplier, "Multiplier is negative.");

         return new BeatTimeSpan(MathUtilities.RoundToLong(Beats * multiplier),
                                    MathUtilities.RoundToLong(Ticks * multiplier));
      }

      /// <summary>
      /// Shrinks the current time span by dividing its length by the specified divisor.
      /// </summary>
      /// <param name="divisor">Divisor to shrink the time span by.</param>
      /// <returns>Time span that is the current time span shrinked by the <paramref name="divisor"/>.</returns>
      public ITimeSpan Divide(double divisor)
      {
         ThrowIfArgument.IsNonpositive(nameof(divisor), divisor, "Divisor is zero or negative.");

         return new BeatTimeSpan(MathUtilities.RoundToLong(Beats / divisor),
                                    MathUtilities.RoundToLong(Ticks / divisor));
      }

      /// <summary>
      /// Clones the current time span.
      /// </summary>
      /// <returns>Copy of the current time span.</returns>
      public ITimeSpan Clone()
      {
         return new BeatTimeSpan(Beats, Ticks);
      }

      /// <summary>
      /// Compares the current instance with another object of the same type and returns an integer
      /// that indicates whether the current instance precedes, follows, or occurs in the same
      /// position in the sort order as the other object.
      /// </summary>
      /// <param name="obj">An object to compare with this instance.</param>
      /// <returns>A value that indicates the relative order of the objects being compared. The
      /// return value has these meanings: Value Meaning Less than zero This instance precedes obj
      /// in the sort order. Zero This instance occurs in the same position in the sort order as obj.
      /// Greater than zero This instance follows obj in the sort order.</returns>
      /// <exception cref="ArgumentException"><paramref name="obj"/> is not the same type as this instance.</exception>
      public int CompareTo(object obj)
      {
         if (ReferenceEquals(obj, null))
            return 1;

         var beatTimeSpan = obj as BeatTimeSpan;
         if (ReferenceEquals(beatTimeSpan, null))
            throw new ArgumentException("Time span is of different type.", nameof(obj));

         return CompareTo(beatTimeSpan);
      }

      #endregion

      #region IComparable<BeatTimeSpan>

      /// <summary>
      /// Compares the current instance with another object of the same type and returns an integer
      /// that indicates whether the current instance precedes, follows, or occurs in the same
      /// position in the sort order as the other object.
      /// </summary>
      /// <param name="other">An object to compare with this instance.</param>
      /// <returns>A value that indicates the relative order of the objects being compared. The
      /// return value has these meanings: Value Meaning Less than zero This instance precedes other
      /// in the sort order. Zero This instance occurs in the same position in the sort order as other.
      /// Greater than zero This instance follows other in the sort order.</returns>
      public int CompareTo(BeatTimeSpan other)
      {
         if (ReferenceEquals(other, null))
            return 1;

         var beatsDelta = Beats - other.Beats;
         var ticksDelta = Ticks - other.Ticks;

         return Math.Sign((beatsDelta != 0 ? beatsDelta : ticksDelta));
      }

      #endregion

      #region IEquatable<BeatTimeSpan>

      /// <summary>
      /// Indicates whether the current object is equal to another object of the same type.
      /// </summary>
      /// <param name="other">An object to compare with this object.</param>
      /// <returns>true if the current object is equal to the other parameter; otherwise, false.</returns>
      public bool Equals(BeatTimeSpan other)
      {
         if (ReferenceEquals(this, other))
            return true;

         if (ReferenceEquals(null, other))
            return false;

         return Beats == other.Beats &&
                Ticks == other.Ticks;
      }

      #endregion
   }
}
