// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Collections;
using Avalonia.Input;

using Gnomeshade.Interfaces.Avalonia.Core.Accounts;
using Gnomeshade.Interfaces.Avalonia.Core.Products;
using Gnomeshade.Interfaces.Avalonia.Core.Transactions.Items;
using Gnomeshade.Interfaces.WebApi.Client;

namespace Gnomeshade.Interfaces.Avalonia.Core.Imports;

/// <summary>External data import view model.</summary>
public sealed class ImportViewModel : ViewModelBase
{
	private readonly IGnomeshadeClient _gnomeshadeClient;

	private string? _filePath;
	private string? _userAccount;
	private DataGridItemCollectionView<AccountOverviewRow>? _accounts;
	private DataGridItemCollectionView<ProductRow>? _products;
	private ProductRow? _selectedProduct;
	private DataGridItemCollectionView<TransactionOverview>? _transactions;
	private TransactionOverview? _selectedTransaction;
	private DataGridItemCollectionView<TransactionItemRow>? _items;
	private TransactionItemRow? _selectedItem;

	/// <summary>Initializes a new instance of the <see cref="ImportViewModel"/> class.</summary>
	/// <param name="gnomeshadeClient">Gnomeshade API client.</param>
	public ImportViewModel(IGnomeshadeClient gnomeshadeClient)
	{
		_gnomeshadeClient = gnomeshadeClient;

		PropertyChanged += OnPropertyChanged;
	}

	/// <summary>Raised when a product is selected for editing.</summary>
	public event EventHandler<ProductSelectedEventArgs>? ProductSelected;

	/// <summary>Raised when a transaction item is selected for editing.</summary>
	public event EventHandler<TransactionItemSelectedEventArgs>? TransactionItemSelected;

	/// <summary>Gets or sets the local path of the report file to import.</summary>
	public string? FilePath
	{
		get => _filePath;
		set => SetAndNotifyWithGuard(ref _filePath, value, nameof(FilePath));
	}

	/// <summary>Gets a value indicating whether the information needed for <see cref="ImportAsync"/> is valid.</summary>
	public bool CanImport => !string.IsNullOrWhiteSpace(FilePath);

	/// <summary>Gets or sets the name of the imported user account.</summary>
	public string? UserAccount
	{
		get => _userAccount;
		set => SetAndNotify(ref _userAccount, value, nameof(UserAccount));
	}

	/// <summary>Gets a grid view of all accounts referenced in imported transactions.</summary>
	public DataGridCollectionView? AccountGridView => Accounts ?? default(DataGridCollectionView);

	/// <summary>Gets or sets a typed collection of rows in <see cref="AccountGridView"/>.</summary>
	public DataGridItemCollectionView<AccountOverviewRow>? Accounts
	{
		get => _accounts;
		set => SetAndNotifyWithGuard(ref _accounts, value, nameof(Accounts), nameof(AccountGridView));
	}

	/// <summary>Gets a grid view of all products referenced in imported transactions.</summary>
	public DataGridCollectionView? ProductGridView => Products ?? default(DataGridCollectionView);

	/// <summary>Gets or sets a typed collection of rows in <see cref="ProductGridView"/>.</summary>
	public DataGridItemCollectionView<ProductRow>? Products
	{
		get => _products;
		set => SetAndNotifyWithGuard(ref _products, value, nameof(Products), nameof(ProductGridView));
	}

	/// <summary>Gets or sets the selected product from <see cref="Products"/>.</summary>
	public ProductRow? SelectedProduct
	{
		get => _selectedProduct;
		set => SetAndNotify(ref _selectedProduct, value, nameof(SelectedProduct));
	}

	/// <summary>Gets a grid view of all transaction in the imported report.</summary>
	public DataGridCollectionView? TransactionGridView => Transactions ?? default(DataGridCollectionView);

	/// <summary>Gets or sets a typed collection of rows in <see cref="TransactionGridView"/>.</summary>
	public DataGridItemCollectionView<TransactionOverview>? Transactions
	{
		get => _transactions;
		set => SetAndNotifyWithGuard(ref _transactions, value, nameof(Transactions), nameof(TransactionGridView));
	}

	/// <summary>Gets or sets the selected transaction from <see cref="Transactions"/>.</summary>
	public TransactionOverview? SelectedTransaction
	{
		get => _selectedTransaction;
		set => SetAndNotify(ref _selectedTransaction, value, nameof(SelectedTransaction));
	}

	/// <summary>Gets a grid view of all transaction items in the <see cref="SelectedTransaction"/>.</summary>
	public DataGridCollectionView? ItemGridView => Items ?? default(DataGridCollectionView);

	/// <summary>Gets or sets a typed collection of rows in <see cref="ItemGridView"/>.</summary>
	public DataGridItemCollectionView<TransactionItemRow>? Items
	{
		get => _items;
		set => SetAndNotifyWithGuard(ref _items, value, nameof(Items), nameof(ItemGridView));
	}

	/// <summary>Gets or sets the selected transaction item from <see cref="Items"/>.</summary>
	public TransactionItemRow? SelectedItem
	{
		get => _selectedItem;
		set => SetAndNotify(ref _selectedItem, value, nameof(SelectedItem));
	}

	/// <summary>Imports the located at <see cref="FilePath"/>.</summary>
	/// <exception cref="InvalidOperationException"><see cref="FilePath"/> is null or whitespace.</exception>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task ImportAsync()
	{
		if (string.IsNullOrWhiteSpace(FilePath))
		{
			throw new InvalidOperationException($"Cannot import file because {nameof(FilePath)} is null");
		}

		var file = new FileInfo(FilePath);
		await using var stream = file.OpenRead();
		var result = await _gnomeshadeClient.Import(stream, file.Name);

		var accountNumber = result.UserAccount.Iban ?? result.UserAccount.AccountNumber;
		UserAccount = $"{result.UserAccount.Name}{(accountNumber is null ? null : $"({accountNumber})")}";

		var accounts = result.AccountReferences.Select(reference => reference.Account).ToList();
		var counterparties = await _gnomeshadeClient.GetCounterpartiesAsync().ConfigureAwait(false);
		var userCounterparty = await _gnomeshadeClient.GetMyCounterpartyAsync().ConfigureAwait(false);
		var accountRows = accounts.Translate(counterparties).ToList();
		Accounts = new(accountRows);

		var transactions = result.TransactionReferences.Select(reference => reference.Transaction);
		var transactionRows = transactions.Translate(accounts, counterparties, userCounterparty).ToList();
		Transactions = new(transactionRows);

		var products = result.ProductReferences.Select(reference => reference.Product);
		var productsRows = await _gnomeshadeClient.GetProductRowsAsync(products);
		Products = new(productsRows);
	}

	/// <inheritdoc />
	public override async Task RefreshAsync()
	{
		// todo do not get all accounts
		var accounts = await _gnomeshadeClient.GetAccountsAsync().ConfigureAwait(false);
		var counterparties = await _gnomeshadeClient.GetCounterpartiesAsync().ConfigureAwait(false);
		var userCounterparty = await _gnomeshadeClient.GetMyCounterpartyAsync().ConfigureAwait(false);
		if (Accounts is not null)
		{
			var usedAccounts = accounts.Where(account => Accounts.Any(row => row.Name == account.Name));
			var accountRows = usedAccounts.Translate(counterparties).ToList();
			Accounts = new(accountRows);
		}

		if (Transactions is not null)
		{
			// todo do not get all transactions
			var transactions = await _gnomeshadeClient.GetTransactionsAsync(DateTimeOffset.UnixEpoch, DateTimeOffset.Now);
			var usedTransactions = transactions
				.Where(transaction => Transactions.Any(row => row.Id == transaction.Id));
			var transactionRows = usedTransactions.Translate(accounts, counterparties, userCounterparty).ToList();
			Transactions = new(transactionRows);
		}

		if (Products is not null)
		{
			// todo do not get all products
			var products = await _gnomeshadeClient.GetProductsAsync();
			var usedProducts = products.Where(product => Products.Any(row => row.Id == product.Id));
			var productRows = await _gnomeshadeClient.GetProductRowsAsync(usedProducts).ConfigureAwait(false);
			Products = new(productRows);
		}
	}

	/// <summary>Handles the <see cref="InputElement.DoubleTapped"/> event for <see cref="ProductGridView"/>.</summary>
	public void OnProductDataGridDoubleTapped()
	{
		if (SelectedProduct is null || ProductSelected is null)
		{
			return;
		}

		ProductSelected(this, new(SelectedProduct.Id));
	}

	/// <summary>Handles the <see cref="InputElement.DoubleTapped"/> event for <see cref="ItemGridView"/>.</summary>
	public void OnItemDataGridDoubleTapped()
	{
		if (SelectedItem is null || TransactionItemSelected is null)
		{
			return;
		}

		TransactionItemSelected(this, new(SelectedItem.Id));
	}

	private void OnPropertyChanged(object? sender, PropertyChangedEventArgs args)
	{
		switch (args.PropertyName)
		{
			case nameof(FilePath):
				OnPropertyChanged(nameof(CanImport));
				break;

			case nameof(SelectedTransaction):
				SetItems();
				break;
		}
	}

	private void SetItems()
	{
		if (SelectedTransaction is null)
		{
			Items = null;
			return;
		}

		Items = new(SelectedTransaction.Items as IEnumerable<TransactionItemRow>);
	}
}
