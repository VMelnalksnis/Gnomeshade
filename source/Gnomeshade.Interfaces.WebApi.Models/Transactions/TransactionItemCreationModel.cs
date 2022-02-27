// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel.DataAnnotations;

using JetBrains.Annotations;

namespace Gnomeshade.Interfaces.WebApi.Models.Transactions;

/// <summary>Information needed to create a transaction item.</summary>
[PublicAPI]
public sealed record TransactionItemCreationModel
{
	/// <inheritdoc cref="TransactionItem.SourceAmount"/>
	[Required]
	public decimal? SourceAmount { get; init; }

	/// <summary>The id of the account from which money was withdrawn.</summary>
	[Required]
	public Guid? SourceAccountId { get; init; }

	/// <inheritdoc cref="TransactionItem.TargetAmount"/>
	[Required]
	public decimal? TargetAmount { get; init; }

	/// <summary>The id of the account into which money was deposited.</summary>
	[Required]
	public Guid? TargetAccountId { get; init; }

	/// <summary>The id of the product that was purchased.</summary>
	[Required]
	public Guid? ProductId { get; init; }

	/// <inheritdoc cref="TransactionItem.Amount"/>
	[Required]
	public decimal? Amount { get; init; }

	/// <inheritdoc cref="TransactionItem.BankReference"/>
	public string? BankReference { get; init; }

	/// <inheritdoc cref="TransactionItem.ExternalReference"/>
	public string? ExternalReference { get; init; }

	/// <inheritdoc cref="TransactionItem.InternalReference"/>
	public string? InternalReference { get; init; }

	/// <inheritdoc cref="TransactionItem.DeliveryDate"/>
	public DateTimeOffset? DeliveryDate { get; init; }

	/// <inheritdoc cref="TransactionItem.Description"/>
	public string? Description { get; init; }
}
