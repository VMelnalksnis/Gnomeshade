// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using Avalonia.Controls;

using Gnomeshade.Interfaces.WebApi.Models.Accounts;
using Gnomeshade.Interfaces.WebApi.Models.Products;

namespace Gnomeshade.Avalonia.Core.Transactions.Controls;

/// <summary>Values for filtering transactions.</summary>
public sealed class TransactionFilter : FilterBase<TransactionOverview>
{
	private static readonly string[] _isValidNames = { nameof(IsValid) };

	private DateTimeOffset? _fromDate;
	private DateTimeOffset? _toDate;
	private Account? _selectedAccount;
	private List<Account> _accounts = new();
	private List<Counterparty> _counterparties = new();
	private Counterparty? _selectedCounterparty;
	private List<Product> _products = new();
	private Product? _selectedProduct;
	private bool _invertAccount;
	private bool _invertCounterparty;
	private bool _invertProduct;

	/// <summary>Gets or sets the date from which to get transactions.</summary>
	public DateTimeOffset? FromDate
	{
		get => _fromDate;
		set => SetAndNotifyWithGuard(ref _fromDate, value, nameof(FromDate), _isValidNames);
	}

	/// <summary>Gets or sets the date until which to ge transactions.</summary>
	public DateTimeOffset? ToDate
	{
		get => _toDate;
		set => SetAndNotifyWithGuard(ref _toDate, value, nameof(ToDate), _isValidNames);
	}

	/// <summary>Gets a value indicating whether the current values are valid search parameters.</summary>
	public bool IsValid => ToDate is null || FromDate is null || ToDate >= FromDate;

	/// <summary>Gets a delegate for formatting an account in an <see cref="AutoCompleteBox"/>.</summary>
	public AutoCompleteSelector<object> AccountSelector => AutoCompleteSelectors.Account;

	/// <summary>Gets a delegate for formatting an counterparty in an <see cref="AutoCompleteBox"/>.</summary>
	public AutoCompleteSelector<object> CounterpartySelector => AutoCompleteSelectors.Counterparty;

	/// <summary>Gets a delegate for formatting an product in an <see cref="AutoCompleteBox"/>.</summary>
	public AutoCompleteSelector<object> ProductSelector => AutoCompleteSelectors.Product;

	/// <summary>Gets or sets a collection of all active accounts.</summary>
	public List<Account> Accounts
	{
		get => _accounts;
		set => SetAndNotify(ref _accounts, value);
	}

	/// <summary>Gets or sets the selected account from <see cref="Accounts"/>.</summary>
	public Account? SelectedAccount
	{
		get => _selectedAccount;
		set => SetAndNotify(ref _selectedAccount, value);
	}

	/// <summary>Gets or sets a value indicating whether to filter transactions with or without <see cref="SelectedAccount"/>.</summary>
	public bool InvertAccount
	{
		get => _invertAccount;
		set => SetAndNotify(ref _invertAccount, value);
	}

	/// <summary>Gets or sets a collection of all counterparties.</summary>
	public List<Counterparty> Counterparties
	{
		get => _counterparties;
		set => SetAndNotify(ref _counterparties, value);
	}

	/// <summary>Gets or sets the selected counterparty from <see cref="Counterparties"/>.</summary>
	public Counterparty? SelectedCounterparty
	{
		get => _selectedCounterparty;
		set => SetAndNotify(ref _selectedCounterparty, value);
	}

	/// <summary>Gets or sets a value indicating whether to filter transactions with or without <see cref="SelectedCounterparty"/>.</summary>
	public bool InvertCounterparty
	{
		get => _invertCounterparty;
		set => SetAndNotify(ref _invertCounterparty, value);
	}

	/// <summary>Gets or sets a collection of all products.</summary>
	public List<Product> Products
	{
		get => _products;
		set => SetAndNotify(ref _products, value);
	}

	/// <summary>Gets or sets the selected product from <see cref="Products"/>.</summary>
	public Product? SelectedProduct
	{
		get => _selectedProduct;
		set => SetAndNotify(ref _selectedProduct, value);
	}

	/// <summary>Gets or sets a value indicating whether to filter transactions with or without <see cref="SelectedProduct"/>.</summary>
	public bool InvertProduct
	{
		get => _invertProduct;
		set => SetAndNotify(ref _invertProduct, value);
	}

	/// <inheritdoc />
	protected override bool FilterRow(TransactionOverview overview)
	{
		if (SelectedAccount is null && SelectedCounterparty is null && SelectedProduct is null)
		{
			return true;
		}

		return IsAccountSelected(overview) && IsCounterpartySelected(overview) && IsProductSelected(overview);
	}

	private bool IsAccountSelected(TransactionOverview overview)
	{
		if (SelectedAccount is null)
		{
			return true;
		}

		return InvertAccount
			? !overview.Transfers.All(transfer =>
				transfer.UserAccount != SelectedAccount.Name && transfer.OtherAccount != SelectedAccount.Name)
			: overview.Transfers.Any(transfer =>
				transfer.UserAccount == SelectedAccount.Name || transfer.OtherAccount == SelectedAccount.Name);
	}

	private bool IsCounterpartySelected(TransactionOverview overview)
	{
		if (SelectedCounterparty is null)
		{
			return true;
		}

		return InvertCounterparty
			? overview.Transfers.All(transfer => transfer.OtherCounterparty != SelectedCounterparty.Name)
			: overview.Transfers.Any(transfer => transfer.OtherCounterparty == SelectedCounterparty.Name);
	}

	private bool IsProductSelected(TransactionOverview overview)
	{
		if (SelectedProduct is null)
		{
			return true;
		}

		return InvertProduct
			? overview.Purchases.All(purchase => purchase.ProductId != SelectedProduct.Id)
			: overview.Purchases.Any(purchase => purchase.ProductId == SelectedProduct.Id);
	}
}
