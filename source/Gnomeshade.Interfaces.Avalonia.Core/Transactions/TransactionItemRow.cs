// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.Interfaces.WebApi.Models.Accounts;
using Gnomeshade.Interfaces.WebApi.Models.Transactions;

namespace Gnomeshade.Interfaces.Avalonia.Core.Transactions;

/// <summary>Overview of a single transaction item.</summary>
public sealed class TransactionItemRow : PropertyChangedBase
{
	/// <summary>Initializes a new instance of the <see cref="TransactionItemRow"/> class.</summary>
	/// <param name="item">The API model from which to create the overview.</param>
	/// <param name="sourceAccount">The account from which money was withdrawn.</param>
	/// <param name="sourceCurrency">The currency of the withdrawn amount.</param>
	/// <param name="targetAccount">The account into which money was deposited.</param>
	/// <param name="targetCurrency">The currency of the deposited amount.</param>
	public TransactionItemRow(
		TransactionItem item,
		Account sourceAccount,
		Currency sourceCurrency,
		Account targetAccount,
		Currency targetCurrency)
	{
		Id = item.Id;
		SourceAmount = item.SourceAmount;
		TargetAmount = item.TargetAmount;
		Product = item.Product.Name;
		Amount = item.Amount;
		Description = item.Description;
		SourceAmount = item.SourceAmount;
		TargetAmount = item.TargetAmount;
		BankReference = item.BankReference;
		ExternalReference = item.ExternalReference;
		InternalReference = item.InternalReference;
		SourceAccount = sourceAccount.Name;
		TargetAccount = targetAccount.Name;
		SourceCurrency = sourceCurrency.AlphabeticCode;
		TargetCurrency = targetCurrency.AlphabeticCode;
	}

	/// <summary>Gets the id of the transaction item.</summary>
	public Guid Id { get; }

	/// <summary>Gets the amount withdrawn from the source account for this item.</summary>
	public decimal SourceAmount { get; }

	/// <summary>Gets the name of the source account.</summary>
	public string SourceAccount { get; }

	/// <summary>Gets the alphabetic code of the source currency.</summary>
	public string SourceCurrency { get; }

	/// <summary>Gets the amount deposited into the target account for this item.</summary>
	public decimal TargetAmount { get; }

	/// <summary>Gets the name of the target account.</summary>
	public string TargetAccount { get; }

	/// <summary>Gets the alphabetic code of the target currency.</summary>
	public string TargetCurrency { get; }

	/// <summary>Gets the name of the product purchased.</summary>
	public string Product { get; }

	/// <summary>Gets the amount of product purchased.</summary>
	public decimal Amount { get; }

	/// <summary>Gets the description of this item.</summary>
	public string? Description { get; }

	/// <summary>Gets the bank reference code of this item.</summary>
	public string? BankReference { get; }

	/// <summary>Gets the external reference code of this item.</summary>
	public string? ExternalReference { get; }

	/// <summary>Gets the internal reference code of this item.</summary>
	public string? InternalReference { get; }
}
