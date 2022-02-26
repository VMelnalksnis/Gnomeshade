// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models.Transactions;

namespace Gnomeshade.Interfaces.Avalonia.Core.Transactions;

/// <summary>Splitting a transaction item into multiple items.</summary>
public sealed class TransactionItemSplitViewModel : ViewModelBase
{
	private readonly IGnomeshadeClient _gnomeshadeClient;
	private readonly TransactionItemRow _transactionItemRowToSplit;
	private readonly Guid _transactionId;
	private TransactionItemCreationViewModel? _selectedItem;
	private TransactionItemCreationViewModel? _itemCreation;

	private TransactionItemSplitViewModel(
		IGnomeshadeClient gnomeshadeClient,
		TransactionItemRow transactionItemRowToSplit,
		Guid transactionId)
	{
		_gnomeshadeClient = gnomeshadeClient;
		_transactionItemRowToSplit = transactionItemRowToSplit;
		_transactionId = transactionId;

		Items = new();
		Items.CollectionChanged += ItemsOnCollectionChanged;

		SourceAccount = _transactionItemRowToSplit.SourceAccount;
		SourceAccountCurrency = _transactionItemRowToSplit.SourceCurrency;
		SourceAmount = _transactionItemRowToSplit.SourceAmount;

		TargetAccount = _transactionItemRowToSplit.TargetAccount;
		TargetAccountCurrency = _transactionItemRowToSplit.TargetCurrency;
		TargetAmount = _transactionItemRowToSplit.TargetAmount;

		Product = _transactionItemRowToSplit.Product;
		Amount = _transactionItemRowToSplit.Amount;
		Unit = string.Empty;
	}

	/// <summary>Gets the name of the source account of the item to split.</summary>
	public string SourceAccount { get; }

	/// <summary>Gets the name of the source account currency of the item to split.</summary>
	public string SourceAccountCurrency { get; }

	/// <summary>Gets the source amount of the item to split.</summary>
	public decimal SourceAmount { get; }

	/// <summary>Gets the name of the target account of the item to split.</summary>
	public string TargetAccount { get; }

	/// <summary>Gets the name of the target account currency of the item to split.</summary>
	public string TargetAccountCurrency { get; }

	/// <summary>Gets the target amount of the item to split.</summary>
	public decimal TargetAmount { get; }

	/// <summary>Gets the name of the product of the item to split.</summary>
	public string Product { get; }

	/// <summary>Gets the amount of product of the item to split.</summary>
	public decimal Amount { get; }

	/// <summary>Gets the unit of product of the item to split.</summary>
	public string Unit { get; }

	/// <summary>Gets a collection of transaction items.</summary>
	public ObservableCollection<TransactionItemCreationViewModel> Items { get; }

	/// <summary>Gets or sets the selected item from <see cref="Items"/>.</summary>
	public TransactionItemCreationViewModel? SelectedItem
	{
		get => _selectedItem;
		set => SetAndNotifyWithGuard(ref _selectedItem, value, nameof(SelectedItem));
	}

	/// <summary>Gets or sets the transaction item creation view model.</summary>
	public TransactionItemCreationViewModel? ItemCreation
	{
		get => _itemCreation;
		set
		{
			SetAndNotifyWithGuard(
				ref _itemCreation,
				value,
				nameof(ItemCreation),
				nameof(CanSave));

			if (ItemCreation is not null)
			{
				ItemCreation.PropertyChanged += ItemCreationOnPropertyChanged;
			}
		}
	}

	/// <summary>Gets a value indicating whether <see cref="Items"/> contains any item.</summary>
	public bool HasAnyItems => Items.Any();

	/// <summary>Gets a value indicating whether or not the current changes can be saved.</summary>
	public bool CanSave =>
		Items.All(item =>
			item.SourceCurrency?.AlphabeticCode == _transactionItemRowToSplit.SourceCurrency &&
			item.TargetCurrency?.AlphabeticCode == _transactionItemRowToSplit.TargetCurrency) &&
		Items.Sum(item => item.SourceAmount) == _transactionItemRowToSplit.SourceAmount &&
		Items.Sum(item => item.TargetAmount) == _transactionItemRowToSplit.TargetAmount;

	/// <summary>Gets a value indicating whether a transaction item can be added.</summary>
	public bool CanAddItem => ItemCreation?.CanCreate ?? true;

	/// <summary>Asynchronously creates a new instance of the <see cref="TransactionItemSplitViewModel"/> class.</summary>
	/// <param name="gnomeshadeClient">API client for getting finance data.</param>
	/// <param name="transactionItemRowToSplit">The transaction item to split into multiple.</param>
	/// <param name="transactionId">The id of the transaction to modify.</param>
	/// <returns>A new instance of <see cref="TransactionItemSplitViewModel"/>.</returns>
	public static Task<TransactionItemSplitViewModel> CreateAsync(
		IGnomeshadeClient gnomeshadeClient,
		TransactionItemRow transactionItemRowToSplit,
		Guid transactionId)
	{
		return Task.FromResult(
			new TransactionItemSplitViewModel(gnomeshadeClient, transactionItemRowToSplit, transactionId));
	}

	/// <summary>Adds a transaction item with information from <see cref="ItemCreation"/> to <see cref="Items"/>.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task AddItemAsync()
	{
		var itemCreationModelTask = TransactionItemCreationViewModel.CreateAsync(_gnomeshadeClient);
		if (ItemCreation is null)
		{
			ItemCreation = await itemCreationModelTask;
			return;
		}

		Items.Add(ItemCreation);
		ItemCreation = await itemCreationModelTask;
	}

	/// <summary>Creates the new items and deletes the old one.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task SaveAsync()
	{
		foreach (var creationViewModel in Items)
		{
			var itemToAdd = new TransactionItemCreationModel
			{
				SourceAmount = creationViewModel.SourceAmount,
				SourceAccountId = creationViewModel.SourceAccount!.Currencies.Single(inCurrency =>
					inCurrency.Currency.Id == creationViewModel.SourceCurrency!.Id).Id,
				TargetAmount = creationViewModel.TargetAmount,
				TargetAccountId = creationViewModel.TargetAccount!.Currencies.Single(inCurrency =>
					inCurrency.Currency.Id == creationViewModel.TargetCurrency!.Id).Id,
				ProductId = creationViewModel.Product!.Id,
				Amount = creationViewModel.Amount,
				BankReference = creationViewModel.BankReference,
				ExternalReference = creationViewModel.ExternalReference,
				InternalReference = creationViewModel.InternalReference,
			};

			await _gnomeshadeClient.PutTransactionItemAsync(Guid.NewGuid(), _transactionId, itemToAdd);
		}

		await _gnomeshadeClient.DeleteTransactionItemAsync(_transactionItemRowToSplit.Id);
	}

	private void ItemsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		OnPropertyChanged(nameof(CanSave));
		OnPropertyChanged(nameof(HasAnyItems));
	}

	private void ItemCreationOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName is nameof(TransactionItemCreationViewModel.CanCreate))
		{
			OnPropertyChanged(nameof(CanSave));
		}
	}
}
