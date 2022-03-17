// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models.Transactions;

namespace Gnomeshade.Interfaces.Avalonia.Core.Transactions;

/// <summary>View model for creating a transaction.</summary>
public sealed class TransactionCreationViewModel : ViewModelBase
{
	private readonly IGnomeshadeClient _gnomeshadeClient;

	private string? _description;
	private DateTimeOffset? _bookingDate;
	private TimeSpan? _bookingTime;
	private DateTimeOffset? _valueDate;
	private TimeSpan? _valueTime;
	private TransactionItemCreationViewModel? _itemCreation;
	private string? _errorMessage;
	private TransactionItemCreationViewModel? _selectedItem;

	/// <summary>Initializes a new instance of the <see cref="TransactionCreationViewModel"/> class.</summary>
	/// <param name="gnomeshadeClient">Finance API client for getting/saving data.</param>
	public TransactionCreationViewModel(IGnomeshadeClient gnomeshadeClient)
	{
		_gnomeshadeClient = gnomeshadeClient;

		Items = new();
		Items.CollectionChanged += ItemsOnCollectionChanged;
	}

	/// <summary>Raised when a new transaction has been successfully created.</summary>
	public event EventHandler<TransactionCreatedEventArgs>? TransactionCreated;

	/// <summary>Gets or sets the description of the transaction.</summary>
	public string? Description
	{
		get => _description;
		set => SetAndNotify(ref _description, value, nameof(Description));
	}

	/// <summary>Gets or sets the date on which the transaction was posted to an account on the account servicer accounting books.</summary>
	public DateTimeOffset? BookingDate
	{
		get => _bookingDate;
		set => SetAndNotifyWithGuard(ref _bookingDate, value, nameof(BookingDate), nameof(CanCreate));
	}

	/// <summary>Gets or sets the time at which the transaction was posted to an account on the account servicer accounting books.</summary>
	public TimeSpan? BookingTime
	{
		get => _bookingTime;
		set => SetAndNotifyWithGuard(ref _bookingTime, value, nameof(BookingTime), nameof(CanCreate));
	}

	/// <summary>Gets or sets the date on which assets become available in case of deposit, or when assets cease to be available in case of withdrawal.</summary>
	public DateTimeOffset? ValueDate
	{
		get => _valueDate;
		set => SetAndNotifyWithGuard(ref _valueDate, value, nameof(ValueDate), nameof(CanCreate));
	}

	/// <summary>Gets or sets the time at which assets become available in case of deposit, or when assets cease to be available in case of withdrawal.</summary>
	public TimeSpan? ValueTime
	{
		get => _valueTime;
		set => SetAndNotifyWithGuard(ref _valueTime, value, nameof(ValueTime), nameof(CanCreate));
	}

	/// <summary>Gets a collection of transaction items.</summary>
	public ObservableCollection<TransactionItemCreationViewModel> Items { get; }

	/// <summary>Gets or sets the selected item from <see cref="Items"/>.</summary>
	public TransactionItemCreationViewModel? SelectedItem
	{
		get => _selectedItem;
		set => SetAndNotifyWithGuard(ref _selectedItem, value, nameof(SelectedItem), nameof(CanDeleteItem));
	}

	/// <summary>Gets a value indicating whether <see cref="Items"/> contains any item.</summary>
	public bool HasAnyItems => Items.Any();

	/// <summary>Gets a value indicating whether a transaction item can be deleted.</summary>
	public bool CanDeleteItem => SelectedItem is not null;

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
				nameof(CanDeleteItem),
				nameof(CanAddItem));

			if (ItemCreation is not null)
			{
				ItemCreation.PropertyChanged += ItemCreationOnPropertyChanged;
			}
		}
	}

	/// <summary>Gets a value indicating whether a new transaction can be created with the given information.</summary>
	public bool CanCreate =>
		((BookingDate.HasValue && BookingTime.HasValue) || (ValueDate.HasValue && ValueTime.HasValue)) &&
		Items.Any();

	/// <summary>Gets or sets the transaction creation error message.</summary>
	public string? ErrorMessage
	{
		get => _errorMessage;
		set => SetAndNotifyWithGuard(ref _errorMessage, value, nameof(ErrorMessage), nameof(IsErrorMessageVisible));
	}

	/// <summary>Gets a value indicating whether the error message should be displayed.</summary>
	public bool IsErrorMessageVisible => !string.IsNullOrWhiteSpace(ErrorMessage);

	/// <summary>Gets a value indicating whether a transaction item can be added.</summary>
	public bool CanAddItem => ItemCreation?.CanCreate ?? true;

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

	/// <summary>Removes the <see cref="SelectedItem"/> from <see cref="Items"/>.</summary>
	public void DeleteItem()
	{
		if (SelectedItem is not null)
		{
			Items.Remove(SelectedItem);
		}
	}

	/// <summary>Attempts to create a new transaction with the given information.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task CreateTransactionAsync()
	{
		var bookedAt = BookingDate.HasValue
			? new DateTimeOffset(BookingDate.Value.Date).Add(BookingTime.GetValueOrDefault())
			: default(DateTimeOffset?);

		var valuedAt = ValueDate.HasValue
			? new DateTimeOffset(ValueDate.Value.Date).Add(ValueTime.GetValueOrDefault())
			: default(DateTimeOffset?);

		var transactionCreationModel = new TransactionCreationModel
		{
			BookedAt = bookedAt,
			ValuedAt = valuedAt,
			Description = Description,
			Items = Items.Select(item => new TransactionItemCreationModel
			{
				// todo currency not yet added to account
				SourceAccountId = item.SourceAccount?.Currencies
					.Single(currency => item.SourceCurrency?.Id == currency.Currency.Id).Id,
				SourceAmount = item.SourceAmount,
				TargetAccountId = item.TargetAccount?.Currencies
					.Single(currency => item.TargetCurrency?.Id == currency.Currency.Id).Id,
				TargetAmount = item.TargetAmount,
				ProductId = item.Product?.Id,
				Amount = item.Amount,
				BankReference = string.IsNullOrWhiteSpace(item.BankReference) ? null : item.BankReference,
				ExternalReference = string.IsNullOrWhiteSpace(item.ExternalReference) ? null : item.ExternalReference,
				InternalReference = string.IsNullOrWhiteSpace(item.InternalReference) ? null : item.InternalReference,
			}).ToList(),
		};

		try
		{
			ErrorMessage = default;
			var id = await _gnomeshadeClient.CreateTransactionAsync(transactionCreationModel).ConfigureAwait(false);
			OnTransactionCreated(id);
		}
		catch (HttpRequestException httpException)
		{
			// todo descriptive error messages
			ErrorMessage = httpException.Message;
		}
	}

	private void OnTransactionCreated(Guid id)
	{
		TransactionCreated?.Invoke(this, new(id));
	}

	private void ItemsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		OnPropertyChanged(nameof(CanCreate));
		OnPropertyChanged(nameof(HasAnyItems));
	}

	private void ItemCreationOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(TransactionItemCreationViewModel.CanCreate))
		{
			OnPropertyChanged(nameof(CanAddItem));
		}
	}
}
