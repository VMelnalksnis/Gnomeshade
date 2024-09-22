// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel.DataAnnotations;

using JetBrains.Annotations;

namespace Gnomeshade.WebApi.Models.Transactions;

/// <summary>Information needed to create a planned transaction.</summary>
/// <seealso cref="PlannedTransaction"/>
[PublicAPI]
public sealed record PlannedTransactionCreation : Creation
{
	/// <inheritdoc cref="PlannedTransaction.ScheduleId"/>
	[Required]
	public Guid? ScheduleId { get; set; }
}
