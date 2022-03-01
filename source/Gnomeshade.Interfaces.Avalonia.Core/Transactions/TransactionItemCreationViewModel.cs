// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Collections;
using Avalonia.Controls;

using Gnomeshade.Interfaces.Avalonia.Core.Products;
using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models.Accounts;
using Gnomeshade.Interfaces.WebApi.Models.Products;
using Gnomeshade.Interfaces.WebApi.Models.Tags;
using Gnomeshade.Interfaces.WebApi.Models.Transactions;

namespace Gnomeshade.Interfaces.Avalonia.Core.Transactions;

/// <summary>
/// Form for creating a single new transaction item.
/// </summary>
public class TransactionItemCreationViewModel : ViewModelBase
{
	private readonly IGnomeshadeClient _transactionClient;
	private readonly TransactionItem? _existingItem;

	private Account? _sourceAccount;
	private decimal? _sourceAmount;
	private Currency? _sourceCurrency;
	private Account? _targetAccount;
	private decimal? _targetAmount;
	private Currency? _targetCurrency;
	private Product? _product;
	private decimal? _amount;
	private string? _bankReference;
	private string? _externalReference;
	private string? _internalReference;
	private ProductCreationViewModel? _productCreation;
	private List<Product> _products;
	private Tag _tag;

	private TransactionItemCreationViewModel(
		IGnomeshadeClient transactionClient,
		List<Account> accounts,
		List<Currency> currencies,
		List<Product> products,
		List<Tag> tags)
	{
		_transactionClient = transactionClient;
		Accounts = accounts;
		Currencies = currencies;
		Tags = tags;
		_products = products;

		AccountSelector = (_, item) => ((Account)item).Name;
		CurrencySelector = (_, item) => ((Currency)item).AlphabeticCode;
		ProductSelector = (_, item) => ((Product)item).Name;
		TagSelector = (_, item) => ((Tag)item).Name;
	}

	private TransactionItemCreationViewModel(
		IGnomeshadeClient transactionClient,
		List<Account> accounts,
		List<Currency> currencies,
		List<Product> products,
		List<Tag> tags,
		TransactionItem item)
		: this(transactionClient, accounts, currencies, products, tags)
	{
		_existingItem = item;

		SourceAccount = Accounts
			.Single(account => account.Currencies.Any(inCurrency => inCurrency.Id == item.SourceAccountId));
		SourceAmount = item.SourceAmount;
		TargetAccount = Accounts
			.Single(account => account.Currencies.Any(inCurrency => inCurrency.Id == item.TargetAccountId));
		TargetAmount = item.TargetAmount;
		Product = Products.Single(product => product.Id == item.Product.Id);
		Amount = item.Amount;
		BankReference = item.BankReference;
		ExternalReference = item.ExternalReference;
		InternalReference = item.InternalReference;
	}

	/// <summary>
	/// Raised when a new transaction item has been successfully created.
	/// </summary>
	public event EventHandler<TransactionItemCreatedEventArgs>? TransactionItemCreated;

	/// <summary>
	/// Gets a collection of all active accounts.
	/// </summary>
	public List<Account> Accounts { get; }

	/// <summary>
	/// Gets a delegate for formatting an account in an <see cref="AutoCompleteBox"/>.
	/// </summary>
	public AutoCompleteSelector<object> AccountSelector { get; }

	/// <summary>
	/// Gets a collection of all currencies.
	/// </summary>
	public List<Currency> Currencies { get; }

	/// <summary>
	/// Gets a delegate for formatting a currency in an <see cref="AutoCompleteBox"/>.
	/// </summary>
	public AutoCompleteSelector<object> CurrencySelector { get; }

	/// <summary>
	/// Gets a collection of all products.
	/// </summary>
	public List<Product> Products
	{
		get => _products;
		private set => SetAndNotify(ref _products, value, nameof(Products));
	}

	/// <summary>
	/// Gets a delegate for formatting a product in an <see cref="AutoCompleteBox"/>.
	/// </summary>
	public AutoCompleteSelector<object> ProductSelector { get; }

	/// <summary>
	/// Gets or sets the source account of the transaction item.
	/// </summary>
	public Account? SourceAccount
	{
		get => _sourceAccount;
		set
		{
			SetAndNotifyWithGuard(ref _sourceAccount, value, nameof(SourceAccount), nameof(CanCreate));
			if (SourceAccount is not null && SourceCurrency is null)
			{
				SourceCurrency = SourceAccount.PreferredCurrency;
			}
		}
	}

	/// <summary>
	/// Gets or sets the amount withdrawn from <see cref="SourceAccount"/>.
	/// </summary>
	public decimal? SourceAmount
	{
		get => _sourceAmount;
		set
		{
			SetAndNotifyWithGuard(ref _sourceAmount, value, nameof(SourceAmount), nameof(CanCreate));
			if (IsTargetAmountReadOnly)
			{
				TargetAmount = SourceAmount;
			}
		}
	}

	/// <summary>
	/// Gets or sets the currency of <see cref="SourceAmount"/>.
	/// </summary>
	public Currency? SourceCurrency
	{
		get => _sourceCurrency;
		set => SetAndNotifyWithGuard(
			ref _sourceCurrency,
			value,
			nameof(SourceCurrency),
			nameof(CanCreate),
			nameof(IsTargetAmountReadOnly));
	}

	/// <summary>
	/// Gets or sets the target account of the transaction item.
	/// </summary>
	public Account? TargetAccount
	{
		get => _targetAccount;
		set
		{
			SetAndNotifyWithGuard(ref _targetAccount, value, nameof(TargetAccount), nameof(CanCreate));
			if (TargetAccount is null || TargetCurrency is not null)
			{
				return;
			}

			TargetCurrency = TargetAccount.PreferredCurrency;
			if (TargetCurrency == SourceCurrency)
			{
				TargetAmount = SourceAmount;
			}
		}
	}

	/// <summary>
	/// Gets or sets the amount deposited to <see cref="TargetAccount"/>.
	/// </summary>
	public decimal? TargetAmount
	{
		get => _targetAmount;
		set => SetAndNotifyWithGuard(ref _targetAmount, value, nameof(TargetAmount), nameof(CanCreate));
	}

	/// <summary>
	/// Gets a value indicating whether <see cref="TargetAmount"/> should not be editable.
	/// </summary>
	public bool IsTargetAmountReadOnly => SourceCurrency == TargetCurrency;

	/// <summary>
	/// Gets or sets the currency of <see cref="TargetAmount"/>.
	/// </summary>
	public Currency? TargetCurrency
	{
		get => _targetCurrency;
		set => SetAndNotifyWithGuard(
			ref _targetCurrency,
			value,
			nameof(TargetCurrency),
			nameof(CanCreate),
			nameof(IsTargetAmountReadOnly));
	}

	/// <summary>
	/// Gets or sets the product of the transaction item.
	/// </summary>
	public Product? Product
	{
		get => _product;
		set => SetAndNotifyWithGuard(ref _product, value, nameof(Product), nameof(CanCreate));
	}

	/// <summary>
	/// Gets or sets the amount of <see cref="Product"/>.
	/// </summary>
	public decimal? Amount
	{
		get => _amount;
		set => SetAndNotifyWithGuard(ref _amount, value, nameof(Amount), nameof(CanCreate));
	}

	/// <summary>
	/// Gets or sets the bank reference of the transaction item.
	/// </summary>
	public string? BankReference
	{
		get => _bankReference;
		set => SetAndNotify(ref _bankReference, value, nameof(BankReference));
	}

	/// <summary>
	/// Gets or sets the external reference of the transaction item.
	/// </summary>
	public string? ExternalReference
	{
		get => _externalReference;
		set => SetAndNotify(ref _externalReference, value, nameof(ExternalReference));
	}

	/// <summary>
	/// Gets or sets the internal reference of the transaction item.
	/// </summary>
	public string? InternalReference
	{
		get => _internalReference;
		set => SetAndNotify(ref _internalReference, value, nameof(InternalReference));
	}

	public List<Tag> Tags { get; set; }

	public DataGridCollectionView TagsToAdd { get; set; }

	public AutoCompleteSelector<object> TagSelector { get; }

	public Tag Tag
	{
		get => _tag;
		set => SetAndNotify(ref _tag, value);
	}

	/// <summary>
	/// Gets a view model for creating a new product.
	/// </summary>
	public ProductCreationViewModel? ProductCreation
	{
		get => _productCreation;
		private set => SetAndNotifyWithGuard(ref _productCreation, value, nameof(ProductCreation), nameof(CanCreateProduct));
	}

	/// <summary>
	/// Gets a value indicating whether the transaction item can be created.
	/// </summary>
	public bool CanCreate =>
		SourceAccount is not null &&
		SourceAmount.HasValue &&
		SourceCurrency is not null &&
		TargetAccount is not null &&
		TargetAmount.HasValue &&
		TargetCurrency is not null &&
		Product is not null &&
		_amount.HasValue;

	/// <summary>
	/// Gets a value indicating whether Update/Save button should be visible.
	/// </summary>
	public bool CanUpdate => _existingItem is not null;

	public bool CanCreateProduct => ProductCreation is null;

	/// <summary>
	/// Asynchronously creates a new instance of the <see cref="TransactionItemCreationViewModel"/> class.
	/// </summary>
	/// <param name="gnomeshadeClient">Gnomeshade API client.</param>
	/// <param name="itemId">The id of the transaction item to edit.</param>
	/// <returns>A new instance of the <see cref="TransactionItemCreationViewModel"/> class.</returns>
	public static async Task<TransactionItemCreationViewModel> CreateAsync(
		IGnomeshadeClient gnomeshadeClient,
		Guid? itemId = null)
	{
		var accounts = await gnomeshadeClient.GetActiveAccountsAsync();
		var currencies = await gnomeshadeClient.GetCurrenciesAsync();
		var products = await gnomeshadeClient.GetProductsAsync();
		var tags = await gnomeshadeClient.GetTagsAsync();

		if (itemId is null)
		{
			return new(gnomeshadeClient, accounts, currencies, products, tags);
		}

		var existingItem = await gnomeshadeClient.GetTransactionItemAsync(itemId.Value);
		return new(gnomeshadeClient, accounts, currencies, products, tags, existingItem);
	}

	/// <summary>
	/// Updates an existing transaction item with the provided values.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	/// <exception cref="InvalidOperationException">One of the required values is not set.</exception>
	public async Task SaveAsync()
	{
		if (_existingItem is null ||
			SourceAccount is null || SourceCurrency is null ||
			TargetAccount is null || TargetCurrency is null ||
			Product is null)
		{
			throw new InvalidOperationException();
		}

		var creationModel = new TransactionItemCreationModel
		{
			SourceAmount = SourceAmount,
			SourceAccountId = SourceAccount.Currencies
				.Single(inCurrency => inCurrency.Currency.Id == SourceCurrency.Id).Id,
			TargetAmount = TargetAmount,
			TargetAccountId = TargetAccount.Currencies
				.Single(inCurrency => inCurrency.Currency.Id == TargetCurrency.Id).Id,
			ProductId = Product.Id,
			Amount = Amount,
			BankReference = BankReference,
			ExternalReference = ExternalReference,
			InternalReference = InternalReference,
			DeliveryDate = null,
			Description = null,
		};

		await _transactionClient.PutTransactionItemAsync(_existingItem.Id, _existingItem.TransactionId, creationModel);
		TransactionItemCreated?.Invoke(this, new(_existingItem.Id));
	}

	/// <summary>
	/// Initializes <see cref="ProductCreation"/> so that a new product can be created.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task CreateProductAsync()
	{
		var creationViewModel = await ProductCreationViewModel.CreateAsync(_transactionClient);
		creationViewModel.ProductCreated += OnProductCreated;
		ProductCreation = creationViewModel;
	}

	private void OnProductCreated(object? sender, ProductCreatedEventArgs e)
	{
		Products = Task.Run(() => _transactionClient.GetProductsAsync()).Result;
		ProductCreation!.ProductCreated -= OnProductCreated;
		ProductCreation = null;
	}
}
