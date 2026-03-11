// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.WebApi.Models.Transactions;

using NodaTime;

namespace Gnomeshade.Avalonia.Core.Transactions;

/// <summary>An overview of a single <see cref="TransactionSchedule"/>.</summary>
public sealed class TransactionScheduleOverview : PropertyChangedBase
{
	/// <summary>Initializes a new instance of the <see cref="TransactionScheduleOverview"/> class.</summary>
	/// <param name="schedule">The schedule this overview will represent.</param>
	/// <param name="timeZone">The current time zone.</param>
	public TransactionScheduleOverview(TransactionSchedule schedule, DateTimeZone timeZone)
	{
		Name = schedule.Name;
		StartingAt = schedule.StartingAt.InZone(timeZone).LocalDateTime;
		Period = schedule.Period;
		Count = schedule.Count;
		Id = schedule.Id;
	}

	/// <summary>Gets the name of the schedule.</summary>
	public string Name { get; }

	/// <summary>Gets the time of the first planned transaction.</summary>
	public LocalDateTime StartingAt { get; }

	/// <summary>Gets the period between each planned transaction.</summary>
	public Period Period { get; }

	/// <summary>Gets the number of planned transactions.</summary>
	public int Count { get; }

	internal Guid Id { get; }
}
