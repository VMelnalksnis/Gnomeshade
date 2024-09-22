// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

namespace Gnomeshade.Data.Entities;

/// <summary>A <see cref="TransactionEntity"/> that will happen in the future.</summary>
public record PlannedTransactionEntity : TransactionBase
{
	/// <summary>Gets or sets the id of the schedule of the planned transaction.</summary>
	/// <seealso cref="TransactionScheduleEntity"/>
	public Guid ScheduleId { get; set; }
}
