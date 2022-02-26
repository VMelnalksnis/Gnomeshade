// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using Gnomeshade.Interfaces.WebApi.Models.Accounts;
using Gnomeshade.Interfaces.WebApi.Models.Transactions;

namespace Gnomeshade.Interfaces.Avalonia.Core.Transactions;

/// <summary>A summary of a single transaction.</summary>
public sealed class TransactionOverview : PropertyChangedBase
{
	private bool _selected;

	/// <summary>Initializes a new instance of the <see cref="TransactionOverview"/> class.</summary>
	/// <param name="transaction">The transaction for which to create an overview.</param>
	/// <param name="sourceAccount">The account from which money was withdrawn.</param>
	/// <param name="sourceCounterparty">The counterparty of source account.</param>
	/// <param name="sourceCurrency">The currency of the withdrawn amount.</param>
	/// <param name="sourceAmount">The amount withdrawn from source account.</param>
	/// <param name="targetAccount">The account into which money was deposited.</param>
	/// <param name="targetCounterparty">The counterparty of target account.</param>
	/// <param name="targetCurrency">The currency of the deposited amount.</param>
	/// <param name="targetAmount">The amount deposited into target account.</param>
	public TransactionOverview(
		Transaction transaction,
		Account sourceAccount,
		Counterparty? sourceCounterparty,
		Currency sourceCurrency,
		decimal sourceAmount,
		Account targetAccount,
		Counterparty? targetCounterparty,
		Currency targetCurrency,
		decimal targetAmount)
	{
		Id = transaction.Id;
		Date = transaction.Date.LocalDateTime;
		Description = transaction.Description;
		SourceAccount = sourceCounterparty?.Name ?? sourceAccount.Name;
		TargetAccount = targetCounterparty?.Name ?? targetAccount.Name;
		SourceAmount = sourceAmount;
		TargetAmount = targetAmount;
		Items = transaction.Items
			.Select(item => new TransactionItemRow(
				item,
				sourceAccount,
				sourceCounterparty,
				sourceCurrency,
				targetAccount,
				targetCounterparty,
				targetCurrency))
			.ToList();
	}

	/// <summary>Gets or sets a value indicating whether this transaction is selected.</summary>
	public bool Selected
	{
		get => _selected;
		set => SetAndNotify(ref _selected, value, nameof(Selected));
	}

	/// <summary>Gets the id of the transaction which this overview represents.</summary>
	public Guid Id { get; }

	/// <summary>Gets the book date of the transaction.</summary>
	public DateTime Date { get; }

	/// <summary>Gets the description of the transaction.</summary>
	public string? Description { get; }

	/// <summary>Gets the name of the source account of the transaction.</summary>
	public string SourceAccount { get; }

	/// <summary>Gets the name of the target account of the transaction.</summary>
	public string TargetAccount { get; }

	/// <summary>Gets the amount withdrawn from the source account.</summary>
	public decimal SourceAmount { get; }

	/// <summary>Gets the amount deposited in the target account.</summary>
	public decimal TargetAmount { get; }

	/// <summary>Gets the items of this transaction.</summary>
	public IReadOnlyCollection<TransactionItemRow> Items { get; }
}
