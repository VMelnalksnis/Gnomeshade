// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

namespace Gnomeshade.Interfaces.WebApi.V1_0.Transactions
{
	/// <summary>
	/// A range of time between two points in time.
	/// </summary>
	public readonly struct TimeRange
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TimeRange"/> struct.
		/// </summary>
		/// <param name="start">The start of the time range.</param>
		/// <param name="end">The end of the time range.</param>
		/// <exception cref="ArgumentException"><paramref name="start"/> is less than <paramref name="end"/>.</exception>
		public TimeRange(DateTimeOffset start, DateTimeOffset end)
		{
			if (start > end)
			{
				throw new ArgumentException("Start must be after end", nameof(start));
			}

			Start = start;
			End = end;
		}

		/// <summary>
		/// Gets the start of the time range.
		/// </summary>
		public DateTimeOffset Start { get; }

		/// <summary>
		/// Gets the end of the time range.
		/// </summary>
		public DateTimeOffset End { get; }

		/// <summary>
		/// Creates a new time range from an optional time range with default values based on the current time.
		/// </summary>
		/// <param name="optional">A time range with an unspecified start and/or end.</param>
		/// <param name="currentTime">The current time from which to derive the default values.</param>
		/// <returns>A time range with unspecified values set to defaults.</returns>
		public static TimeRange FromOptional(OptionalTimeRange optional, DateTimeOffset currentTime)
		{
			var toDate = optional.To.GetValueOrDefault(currentTime);
			var defaultFromDate = new DateTimeOffset(toDate.Year, toDate.Month, 01, 00, 00, 00, toDate.Offset);
			var fromDate = optional.From.GetValueOrDefault(defaultFromDate);

			return new(fromDate, toDate);
		}

		/// <summary>
		/// Deconstructs the time range into it's start and end points.
		/// </summary>
		/// <param name="start">The start of the time range.</param>
		/// <param name="end">The end of the time range.</param>
		public void Deconstruct(out DateTimeOffset start, out DateTimeOffset end)
		{
			start = Start;
			end = End;
		}
	}
}
