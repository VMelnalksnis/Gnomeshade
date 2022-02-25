// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

namespace Gnomeshade.Interfaces.Avalonia.Core.Transactions;

public sealed class TransactionItem : PropertyChangedBase
{
	private readonly decimal _sourceAmount;
	private readonly string _sourceAccount;
	private readonly decimal _targetAmount;
	private readonly string _targetAccount;
	private readonly string _product;
	private readonly decimal _amount;
	private readonly string _sourceCurrency;
	private readonly string _targetCurrency;
	private readonly string? _bankReference;
	private readonly string? _externalReference;
	private readonly string? _internalReference;
	private readonly Guid _id;

	public Guid Id
	{
		get => _id;
		init => SetAndNotify(ref _id, value, nameof(Id));
	}

	public decimal SourceAmount
	{
		get => _sourceAmount;
		init => SetAndNotify(ref _sourceAmount, value, nameof(SourceAmount));
	}

	public string SourceAccount
	{
		get => _sourceAccount;
		init => SetAndNotify(ref _sourceAccount, value, nameof(SourceAccount));
	}

	public string SourceCurrency
	{
		get => _sourceCurrency;
		init => SetAndNotify(ref _sourceCurrency, value, nameof(SourceCurrency));
	}

	public decimal TargetAmount
	{
		get => _targetAmount;
		init => SetAndNotify(ref _targetAmount, value, nameof(TargetAmount));
	}

	public string TargetAccount
	{
		get => _targetAccount;
		init => SetAndNotify(ref _targetAccount, value, nameof(TargetAccount));
	}

	public string TargetCurrency
	{
		get => _targetCurrency;
		init => SetAndNotify(ref _targetCurrency, value, nameof(TargetCurrency));
	}

	public string Product
	{
		get => _product;
		init => SetAndNotify(ref _product, value, nameof(Product));
	}

	public decimal Amount
	{
		get => _amount;
		init => SetAndNotify(ref _amount, value, nameof(Amount));
	}

	public string? BankReference
	{
		get => _bankReference;
		init => SetAndNotify(ref _bankReference, value);
	}

	public string? ExternalReference
	{
		get => _externalReference;
		init => SetAndNotify(ref _externalReference, value);
	}

	public string? InternalReference
	{
		get => _internalReference;
		init => SetAndNotify(ref _internalReference, value);
	}
}
