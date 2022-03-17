// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Collections;

using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models.Transactions;

namespace Gnomeshade.Interfaces.Avalonia.Core.Transactions;

/// <summary>A detailed view of one transaction and its items.</summary>
public sealed class TransactionDetailViewModel : ViewModelBase
{
	private readonly IGnomeshadeClient _gnomeshadeClient;
	private readonly Guid _initialId;
	private string? _description;
	private DateTimeOffset? _bookingDate;
	private TimeSpan? _bookingTime;
	private DateTimeOffset? _valueDate;
	private TimeSpan? _valueTime;
	private TransactionItemRow? _selectedItem;
	private DataGridItemCollectionView<TransactionItemRow> _items = null!;
	private TransactionItemCreationViewModel _itemCreation;

	private TransactionDetailViewModel(
		IGnomeshadeClient gnomeshadeClient,
		Guid initialId,
		TransactionItemCreationViewModel itemCreationViewModel)
	{
		_gnomeshadeClient = gnomeshadeClient;
		_initialId = initialId;
		_itemCreation = itemCreationViewModel;
		ItemCreation.PropertyChanged += ItemCreationOnPropertyChanged;
	}

	/// <summary>Raised when an item has been selected for splitting.</summary>
	public event EventHandler<TransactionItemSplitEventArgs>? ItemSplit;

	/// <summary>Gets or sets the date on which the transaction was posted to an account on the account servicer accounting books.</summary>
	public DateTimeOffset? BookingDate
	{
		get => _bookingDate;
		set => SetAndNotify(ref _bookingDate, value);
	}

	/// <summary>Gets or sets the time at which the transaction was posted to an account on the account servicer accounting books.</summary>
	public TimeSpan? BookingTime
	{
		get => _bookingTime;
		set => SetAndNotify(ref _bookingTime, value);
	}

	/// <summary>Gets or sets the date on which assets become available in case of deposit, or when assets cease to be available in case of withdrawal.</summary>
	public DateTimeOffset? ValueDate
	{
		get => _valueDate;
		set => SetAndNotify(ref _valueDate, value);
	}

	/// <summary>Gets or sets the time at which assets become available in case of deposit, or when assets cease to be available in case of withdrawal.</summary>
	public TimeSpan? ValueTime
	{
		get => _valueTime;
		set => SetAndNotify(ref _valueTime, value);
	}

	/// <summary>Gets or sets the description of the transaction.</summary>
	public string? Description
	{
		get => _description;
		set => SetAndNotify(ref _description, value);
	}

	/// <summary>Gets all transaction items of the current transaction.</summary>
	/// /// <remarks>
	/// Collection item data binding to source only works when using <see cref="DataGridCollectionView"/>.
	/// Otherwise, it would be nice to use a generic collection and remove <see cref="Items"/>.
	/// </remarks>
	public DataGridCollectionView DataGridView => Items;

	/// <summary>Gets or sets the selected item from <see cref="Items"/>.</summary>
	public TransactionItemRow? SelectedItem
	{
		get => _selectedItem;
		set => SetAndNotifyWithGuard(
			ref _selectedItem,
			value,
			nameof(SelectedItem),
			nameof(CanDeleteItem),
			nameof(CanSplitItem),
			nameof(CanUpdateItem));
	}

	/// <summary>Gets a value indicating whether the <see cref="SelectedItem"/> can be deleted.</summary>
	public bool CanDeleteItem => SelectedItem is not null && Items.Count() > 1;

	/// <summary>Gets a value indicating whether the <see cref="SelectedItem"/> can be split into multiple items.</summary>
	public bool CanSplitItem => SelectedItem is not null;

	/// <summary>Gets a typed collection of all transaction items.</summary>
	public DataGridItemCollectionView<TransactionItemRow> Items
	{
		get => _items;
		private set
		{
			if (_items is not null)
			{
				Items.CollectionChanged -= ItemsOnCollectionChanged;
			}

			SetAndNotifyWithGuard(ref _items, value, nameof(Items), nameof(DataGridView));
			Items.CollectionChanged += ItemsOnCollectionChanged;
		}
	}

	/// <summary>Gets the view model for adding a new item to the current transaction.</summary>
	public TransactionItemCreationViewModel ItemCreation
	{
		get => _itemCreation;
		private set
		{
			ItemCreation.PropertyChanged -= ItemCreationOnPropertyChanged;
			SetAndNotifyWithGuard(ref _itemCreation, value, nameof(ItemCreation), nameof(CanAddItem));
			ItemCreation.PropertyChanged += ItemCreationOnPropertyChanged;
		}
	}

	/// <summary>Gets a value indicating whether the item from <see cref="ItemCreation"/> can be added.</summary>
	public bool CanAddItem => ItemCreation.CanCreate;

	/// <summary>Gets a value indicating whether the <see cref="SelectedItem"/> item can be updated with values from <see cref="ItemCreation"/>.</summary>
	public bool CanUpdateItem => ItemCreation.CanCreate && SelectedItem is not null;

	/// <summary>Asynchronously creates a new instance of the <see cref="TransactionDetailViewModel"/> class.</summary>
	/// <param name="gnomeshadeClient">API client for getting finance data.</param>
	/// <param name="initialId">The id of the initial transaction to display.</param>
	/// <returns>A new instance of <see cref="TransactionDetailViewModel"/>.</returns>
	public static async Task<TransactionDetailViewModel> CreateAsync(
		IGnomeshadeClient gnomeshadeClient,
		Guid initialId)
	{
		var itemViewModel = await TransactionItemCreationViewModel.CreateAsync(gnomeshadeClient);
		var viewModel = new TransactionDetailViewModel(gnomeshadeClient, initialId, itemViewModel);
		await viewModel.GetTransactionAsync(initialId).ConfigureAwait(false);
		return viewModel;
	}

	/// <summary>Updates the transaction with information in this viewmodel.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task UpdateAsync()
	{
		var bookedAt = BookingDate.HasValue
			? new DateTimeOffset(BookingDate.Value.Date).Add(BookingTime.GetValueOrDefault())
			: default(DateTimeOffset?);

		var valuedAt = ValueDate.HasValue
			? new DateTimeOffset(ValueDate.Value.Date).Add(ValueTime.GetValueOrDefault())
			: default(DateTimeOffset?);

		var transaction = await _gnomeshadeClient.GetTransactionAsync(_initialId).ConfigureAwait(false);
		var creationModel = new TransactionCreationModel
		{
			BookedAt = bookedAt,
			Description = Description,
			Items = transaction.Items.Select(item => new TransactionItemCreationModel
			{
				SourceAmount = item.SourceAmount,
				SourceAccountId = item.SourceAccountId,
				TargetAmount = item.TargetAmount,
				TargetAccountId = item.TargetAccountId,
				ProductId = item.Product.Id,
				Amount = item.Amount,
				BankReference = item.BankReference,
				ExternalReference = item.ExternalReference,
				InternalReference = item.InternalReference,
				DeliveryDate = item.DeliveryDate,
				Description = item.Description,
			}).ToList(),
			ValuedAt = valuedAt,
		};

		await _gnomeshadeClient.PutTransactionAsync(_initialId, creationModel).ConfigureAwait(false);
		await GetTransactionAsync(_initialId).ConfigureAwait(false);
	}

	/// <summary>Add a new item from <see cref="ItemCreation"/> to the current transaction.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task AddItemAsync()
	{
		var item = ItemCreation;
		var sourceAccountId = item.SourceAccount?.Currencies
			.Single(currency => item.SourceCurrency?.Id == currency.Currency.Id).Id;
		var targetAccountId = item.TargetAccount?.Currencies
			.Single(currency => item.TargetCurrency?.Id == currency.Currency.Id).Id;

		var creationModel = new TransactionItemCreationModel
		{
			// todo currency not yet added to account
			SourceAccountId = sourceAccountId,
			SourceAmount = item.SourceAmount,
			TargetAccountId = targetAccountId,
			TargetAmount = item.TargetAmount,
			ProductId = item.Product?.Id,
			Amount = item.Amount,
			BankReference = string.IsNullOrWhiteSpace(item.BankReference) ? null : item.BankReference,
			ExternalReference = string.IsNullOrWhiteSpace(item.ExternalReference) ? null : item.ExternalReference,
			InternalReference = string.IsNullOrWhiteSpace(item.InternalReference) ? null : item.InternalReference,
		};

		await _gnomeshadeClient.PutTransactionItemAsync(Guid.NewGuid(), _initialId, creationModel)
			.ConfigureAwait(false);
		ItemCreation.PropertyChanged -= ItemCreationOnPropertyChanged;
		ItemCreation = await TransactionItemCreationViewModel.CreateAsync(_gnomeshadeClient);
		await GetTransactionAsync(_initialId).ConfigureAwait(false);
	}

	/// <summary>Deletes the <see cref="SelectedItem"/>.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task DeleteItemAsync()
	{
		if (SelectedItem is null)
		{
			return;
		}

		await _gnomeshadeClient.DeleteTransactionItemAsync(SelectedItem.Id).ConfigureAwait(false);
		SelectedItem = null;

		await GetTransactionAsync(_initialId).ConfigureAwait(false);
	}

	/// <summary>Splits the <see cref="SelectedItem"/> into multiple items.</summary>
	public void SplitItem()
	{
		ItemSplit?.Invoke(this, new(SelectedItem!, _initialId));
	}

	/// <summary>Updates <see cref="SelectedItem"/> with values from <see cref="ItemCreation"/>.</summary>
	/// <exception cref="InvalidOperationException"><see cref="SelectedItem"/> is <see langword="null"/>.</exception>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task UpdateItemAsync()
	{
		if (SelectedItem is null)
		{
			throw new InvalidOperationException();
		}

		var item = ItemCreation;
		var sourceAccountId = item.SourceAccount?.Currencies
			.Single(currency => item.SourceCurrency?.Id == currency.Currency.Id).Id;
		var targetAccountId = item.TargetAccount?.Currencies
			.Single(currency => item.TargetCurrency?.Id == currency.Currency.Id).Id;

		var creationModel = new TransactionItemCreationModel
		{
			// todo currency not yet added to account
			SourceAccountId = sourceAccountId,
			SourceAmount = item.SourceAmount,
			TargetAccountId = targetAccountId,
			TargetAmount = item.TargetAmount,
			ProductId = item.Product?.Id,
			Amount = item.Amount,
			BankReference = string.IsNullOrWhiteSpace(item.BankReference) ? null : item.BankReference,
			ExternalReference = string.IsNullOrWhiteSpace(item.ExternalReference) ? null : item.ExternalReference,
			InternalReference = string.IsNullOrWhiteSpace(item.InternalReference) ? null : item.InternalReference,
		};

		await _gnomeshadeClient.PutTransactionItemAsync(SelectedItem.Id, _initialId, creationModel)
			.ConfigureAwait(false);
		ItemCreation.PropertyChanged -= ItemCreationOnPropertyChanged;
		ItemCreation = await TransactionItemCreationViewModel.CreateAsync(_gnomeshadeClient);
		await GetTransactionAsync(_initialId).ConfigureAwait(false);
	}

	private async Task GetTransactionAsync(Guid id)
	{
		var transaction = await _gnomeshadeClient.GetTransactionAsync(id).ConfigureAwait(false);
		var accounts = await _gnomeshadeClient.GetAccountsAsync().ConfigureAwait(false);
		var currencies = await _gnomeshadeClient.GetCurrenciesAsync().ConfigureAwait(false);
		var userCounterparty = await _gnomeshadeClient.GetMyCounterpartyAsync().ConfigureAwait(false);

		var items =
			transaction
				.Items
				.Select(async item =>
				{
					var sourceAccount = accounts.Single(account =>
						account.Currencies.Any(currency => currency.Id == item.SourceAccountId));
					var sourceCurrency = currencies.Single(currency =>
						currency.Id ==
						sourceAccount.Currencies
							.SingleOrDefault(inCurrency => inCurrency.Currency.Id == currency.Id)?.Currency.Id);

					var sourceCounterparty = sourceAccount.CounterpartyId == userCounterparty.Id
						? null
						: await _gnomeshadeClient.GetCounterpartyAsync(sourceAccount.CounterpartyId);

					var targetAccount = accounts.Single(account =>
						account.Currencies.Any(currency => currency.Id == item.TargetAccountId));
					var targetCurrency = currencies.Single(currency =>
						currency.Id ==
						targetAccount.Currencies
							.SingleOrDefault(inCurrency => inCurrency.Currency.Id == currency.Id)?.Currency.Id);

					var targetCounterparty = targetAccount.CounterpartyId == userCounterparty.Id
						? null
						: await _gnomeshadeClient.GetCounterpartyAsync(targetAccount.CounterpartyId);

					var tags = await _gnomeshadeClient.GetTransactionItemTagsAsync(item.Id);
					var tagNames = tags.Select(tag => tag.Name).ToList();

					return new TransactionItemRow(item, sourceAccount, sourceCounterparty, sourceCurrency, targetAccount, targetCounterparty, targetCurrency, tagNames);
				})
				.Select(task => task.Result)
				.ToList();

		BookingDate = transaction.BookedAt?.ToLocalTime();
		BookingTime = transaction.BookedAt?.ToLocalTime().TimeOfDay;
		ValueDate = transaction.ValuedAt?.ToLocalTime();
		ValueTime = transaction.ValuedAt?.ToLocalTime().TimeOfDay;
		Description = transaction.Description;

		// ReSharper disable once ConditionIsAlwaysTrueOrFalse
		if (Items is not null)
		{
			Items.CollectionChanged -= ItemsOnCollectionChanged;
		}

		Items = new(items);

		if (ItemCreation.SourceAccount is null && ItemCreation.TargetAccount is null)
		{
			var firstItem = Items.First();
			if (items.All(item => item.SourceAccount == firstItem.SourceAccount))
			{
				ItemCreation.SourceAccount = ItemCreation.Accounts
					.FirstOrDefault(account => account.Name == firstItem.SourceAccount);
			}

			if (items.All(item => item.TargetAccount == firstItem.TargetAccount))
			{
				ItemCreation.TargetAccount = ItemCreation.Accounts
					.FirstOrDefault(account => account.Name == firstItem.TargetAccount);
			}
		}
	}

	private void ItemsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		OnPropertyChanged(nameof(DataGridView));
	}

	private void ItemCreationOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		OnPropertyChanged(nameof(CanAddItem));
		OnPropertyChanged(nameof(CanUpdateItem));
	}
}
