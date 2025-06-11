// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using JetBrains.Annotations;

namespace Gnomeshade.WebApi.Models.Transactions;

/// <summary>A financial transaction during which funds can be transferred between multiple accounts.</summary>
[PublicAPI]
public record PlannedTransaction : TransactionBase
{
	/// <summary>The id of the schedule of this planned transaction.</summary>
	public Guid ScheduleId { get; set; }
}
