// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.WebApi.Models.Transactions;

namespace Gnomeshade.Avalonia.Core.Transactions.Transfers;

/// <summary>Overview of a single <see cref="Transfer"/>.</summary>
public sealed class TransferOverview : PropertyChangedBase
{
	/// <summary>Initializes a new instance of the <see cref="TransferOverview"/> class.</summary>
	/// <param name="id">The id of the transfer.</param>
	/// <param name="sourceAmount">The amount withdrawn from the source account for this transfer.</param>
	/// <param name="sourceAccount">The name of the source account.</param>
	/// <param name="sourceCurrency">The alphabetic code of the source currency.</param>
	/// <param name="targetAmount">The amount deposited into the target account for this transfer.</param>
	/// <param name="targetAccount">The name of the target account.</param>
	/// <param name="targetCurrency">The alphabetic code of the target currency.</param>
	/// <param name="order">The order of the transfer within a transaction.</param>
	/// <param name="bookedAt">The point in time when this transaction was posted to an account on the account servicer accounting books.</param>
	/// <param name="valuedAt">The point in time when assets become available in case of deposit, or when assets cease to be available in case of withdrawal.</param>
	public TransferOverview(
		Guid id,
		decimal sourceAmount,
		string sourceAccount,
		string sourceCurrency,
		decimal targetAmount,
		string targetAccount,
		string targetCurrency,
		uint? order,
		DateTimeOffset? bookedAt,
		DateTimeOffset? valuedAt)
	{
		Id = id;
		SourceAmount = sourceAmount;
		SourceAccount = sourceAccount;
		SourceCurrency = sourceCurrency;
		TargetAmount = targetAmount;
		TargetAccount = targetAccount;
		TargetCurrency = targetCurrency;
		Order = order;
		BookedAt = bookedAt;
		ValuedAt = valuedAt;
	}

	/// <summary>Gets the id of the transfer.</summary>
	public Guid Id { get; }

	/// <summary>Gets the amount withdrawn from the source account for this transfer.</summary>
	public decimal SourceAmount { get; }

	/// <summary>Gets the name of the source account.</summary>
	public string SourceAccount { get; }

	/// <summary>Gets the alphabetic code of the source currency.</summary>
	public string SourceCurrency { get; }

	/// <summary>Gets the amount deposited into the target account for this transfer.</summary>
	public decimal TargetAmount { get; }

	/// <summary>Gets the name of the target account.</summary>
	public string TargetAccount { get; }

	/// <summary>Gets the alphabetic code of the target currency.</summary>
	public string TargetCurrency { get; }

	/// <summary>Gets the order of the transfer within a transaction.</summary>
	public uint? Order { get; }

	/// <summary>Gets the point in time when this transaction was posted to an account on the account servicer accounting books.</summary>
	public DateTimeOffset? BookedAt { get; }

	/// <summary>Gets the point in time when assets become available in case of deposit, or when assets cease to be available in case of withdrawal.</summary>
	public DateTimeOffset? ValuedAt { get; }

	/// <summary>Gets the date of the transaction.</summary>
	public DateTimeOffset? Date => ValuedAt ?? BookedAt;
}
