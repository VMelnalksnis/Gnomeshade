// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.ComponentModel.DataAnnotations;

using JetBrains.Annotations;

using NodaTime;

namespace Gnomeshade.WebApi.Models.Transactions;

/// <summary>Information needed to create a transaction schedule.</summary>
/// <seealso cref="TransactionSchedule"/>
[PublicAPI]
public sealed record TransactionScheduleCreation : Creation
{
	/// <inheritdoc cref="TransactionSchedule.Name"/>
	[Required]
	public string Name { get; set; } = null!;

	/// <inheritdoc cref="TransactionSchedule.StartingAt"/>
	[Required]
	public Instant StartingAt { get; set; }

	/// <inheritdoc cref="TransactionSchedule.Period"/>
	[Required]
	public Period Period { get; set; } = null!;

	/// <inheritdoc cref="TransactionSchedule.Count"/>
	[Required]
	public int Count { get; set; }
}
