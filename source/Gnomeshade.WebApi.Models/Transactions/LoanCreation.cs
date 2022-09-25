﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel.DataAnnotations;

using JetBrains.Annotations;

namespace Gnomeshade.WebApi.Models.Transactions;

/// <summary>Information needed to create a loan.</summary>
/// <seealso cref="Loan"/>
[PublicAPI]
public sealed record LoanCreation : TransactionItemCreation
{
	/// <inheritdoc cref="Loan.TransactionId"/>
	[Required]
	public override Guid? TransactionId { get; set; }

	/// <inheritdoc cref="Loan.IssuingCounterpartyId"/>
	[Required]
	public Guid? IssuingCounterpartyId { get; set; }

	/// <inheritdoc cref="Loan.ReceivingCounterpartyId"/>
	[Required]
	public Guid? ReceivingCounterpartyId { get; set; }

	/// <inheritdoc cref="Loan.Amount"/>
	[Required]
	public decimal? Amount { get; set; }

	/// <inheritdoc cref="Loan.CurrencyId"/>
	[Required]
	public Guid? CurrencyId { get; set; }
}
