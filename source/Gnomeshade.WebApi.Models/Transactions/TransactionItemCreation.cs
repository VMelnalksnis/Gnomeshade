// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel.DataAnnotations;

namespace Gnomeshade.WebApi.Models.Transactions;

/// <summary>Creation model of a sub-item of a <see cref="Transaction"/>.</summary>
public abstract record TransactionItemCreation : Creation
{
	/// <summary>The id of the transaction of which this item is a part of.</summary>
	[Required]
	public abstract Guid? TransactionId { get; set; }
}
