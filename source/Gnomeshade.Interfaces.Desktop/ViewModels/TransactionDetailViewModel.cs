﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Collections;

using Gnomeshade.Interfaces.Desktop.Models;
using Gnomeshade.Interfaces.Desktop.ViewModels.Binding;
using Gnomeshade.Interfaces.Desktop.ViewModels.Design;
using Gnomeshade.Interfaces.Desktop.Views;
using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.V1_0.Transactions;

namespace Gnomeshade.Interfaces.Desktop.ViewModels
{
	/// <summary>
	/// A detailed view of one transaction and its items.
	/// </summary>
	public sealed class TransactionDetailViewModel : ViewModelBase<TransactionDetailView>
	{
		private readonly IGnomeshadeClient _gnomeshadeClient;
		private readonly Guid _initialId;
		private string? _description;
		private DateTimeOffset _date;
		private TransactionItem? _selectedItem;
		private DataGridItemCollectionView<TransactionItem> _items = null!;
		private TransactionItemCreationViewModel _itemCreation = null!;

		/// <summary>
		/// Initializes a new instance of the <see cref="TransactionDetailViewModel"/> class.
		/// Should only be used during design time.
		/// </summary>
		public TransactionDetailViewModel()
			: this(new DesignTimeGnomeshadeClient(), Guid.Empty, new(new DesignTimeGnomeshadeClient()))
		{
			GetTransactionAsync(Guid.Empty).GetAwaiter().GetResult();
		}

		private TransactionDetailViewModel(
			IGnomeshadeClient gnomeshadeClient,
			Guid initialId,
			TransactionItemCreationViewModel itemCreationViewModel)
		{
			_gnomeshadeClient = gnomeshadeClient;
			_initialId = initialId;
			ItemCreation = itemCreationViewModel;
		}

		/// <summary>
		/// Gets or sets the book date of the transaction.
		/// </summary>
		public DateTimeOffset Date
		{
			get => _date;
			set => SetAndNotify(ref _date, value, nameof(Date));
		}

		/// <summary>
		/// Gets or sets the description of the transaction.
		/// </summary>
		public string? Description
		{
			get => _description;
			set => SetAndNotify(ref _description, value, nameof(Description));
		}

		/// <summary>
		/// Gets all transaction items of the current transaction.
		/// </summary>
		/// /// <remarks>
		/// Collection item data binding to source only works when using <see cref="DataGridCollectionView"/>.
		/// Otherwise, it would be nice to use a generic collection and remove <see cref="Items"/>.
		/// </remarks>
		public DataGridCollectionView DataGridView => Items;

		/// <summary>
		/// Gets or sets the selected item from <see cref="Items"/>.
		/// </summary>
		public TransactionItem? SelectedItem
		{
			get => _selectedItem;
			set => SetAndNotifyWithGuard(ref _selectedItem, value, nameof(SelectedItem), nameof(CanDeleteItem));
		}

		/// <summary>
		/// Gets a value indicating whether the <see cref="SelectedItem"/> can be deleted.
		/// </summary>
		public bool CanDeleteItem => SelectedItem is not null && Items.Count() > 1;

		/// <summary>
		/// Gets a typed collection of all transaction items.
		/// </summary>
		public DataGridItemCollectionView<TransactionItem> Items
		{
			get => _items;
			private set
			{
				SetAndNotifyWithGuard(ref _items, value, nameof(Items), nameof(DataGridView));
				Items.CollectionChanged += ItemsOnCollectionChanged;
			}
		}

		/// <summary>
		/// Gets the view model for adding a new item to the current transaction.
		/// </summary>
		public TransactionItemCreationViewModel ItemCreation
		{
			get => _itemCreation;
			private set
			{
				SetAndNotifyWithGuard(ref _itemCreation, value, nameof(ItemCreation), nameof(CanAddItem));
				ItemCreation.PropertyChanged += ItemCreationOnPropertyChanged;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the item from <see cref="ItemCreation"/> can be added.
		/// </summary>
		public bool CanAddItem => ItemCreation.CanCreate;

		/// <summary>
		/// Asynchronously creates a new instance of the <see cref="TransactionDetailViewModel"/> class.
		/// </summary>
		/// <param name="gnomeshadeClient">API client for getting finance data.</param>
		/// <param name="initialId">The id of the initial transaction to display.</param>
		/// <returns>A new instance of <see cref="TransactionDetailViewModel"/>.</returns>
		public static async Task<TransactionDetailViewModel> CreateAsync(
			IGnomeshadeClient gnomeshadeClient,
			Guid initialId)
		{
			var viewModel = new TransactionDetailViewModel(gnomeshadeClient, initialId, new(gnomeshadeClient));
			await viewModel.GetTransactionAsync(initialId).ConfigureAwait(false);
			return viewModel;
		}

		/// <summary>
		/// Add a new item from <see cref="ItemCreation"/> to the current transaction.
		/// </summary>
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

			_ = await _gnomeshadeClient.PutTransactionItemAsync(_initialId, creationModel).ConfigureAwait(false);
			ItemCreation.PropertyChanged -= ItemCreationOnPropertyChanged;
			ItemCreation = new(_gnomeshadeClient);
			await GetTransactionAsync(_initialId).ConfigureAwait(false);
		}

		/// <summary>
		/// Deletes the <see cref="SelectedItem"/>.
		/// </summary>
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

		private async Task GetTransactionAsync(Guid id)
		{
			var transaction = await _gnomeshadeClient.GetTransactionAsync(id).ConfigureAwait(false);
			var accounts = await _gnomeshadeClient.GetAccountsAsync().ConfigureAwait(false);
			var currencies = await _gnomeshadeClient.GetCurrenciesAsync().ConfigureAwait(false);

			var items =
				transaction
					.Items
					.Select(item =>
					{
						var sourceAccount = accounts.Single(account =>
							account.Currencies.Any(currency => currency.Id == item.SourceAccountId));
						var sourceCurrency = currencies.Single(currency =>
							currency.Id ==
							sourceAccount.Currencies
								.SingleOrDefault(inCurrency => inCurrency.Currency.Id == currency.Id)?.Currency.Id);

						var targetAccount = accounts.Single(account =>
							account.Currencies.Any(currency => currency.Id == item.TargetAccountId));
						var targetCurrency = currencies.Single(currency =>
							currency.Id ==
							targetAccount.Currencies
								.SingleOrDefault(inCurrency => inCurrency.Currency.Id == currency.Id)?.Currency.Id);

						return new TransactionItem
						{
							Id = item.Id,
							Amount = item.Amount,
							Product = item.Product.Name,
							SourceAccount = sourceAccount.Name,
							TargetAccount = targetAccount.Name,
							SourceAmount = item.SourceAmount,
							TargetAmount = item.TargetAmount,
							SourceCurrency = sourceCurrency.AlphabeticCode,
							TargetCurrency = targetCurrency.AlphabeticCode,
						};
					}).ToList();

			Date = transaction.Date;
			Description = transaction.Description;

			if (Items is not null!)
			{
				Items.CollectionChanged -= ItemsOnCollectionChanged;
			}

			Items = new(items);

			if (ItemCreation.SourceAccount is null && ItemCreation.TargetAccount is null)
			{
				var firstItem = Items.First();
				if (items.All(item => item.SourceAccount == firstItem.SourceAccount))
				{
					ItemCreation.SourceAccount =
						(await ItemCreation.Accounts.ConfigureAwait(false))
						.FirstOrDefault(account => account.Name == firstItem.SourceAccount);
				}

				if (items.All(item => item.TargetAccount == firstItem.TargetAccount))
				{
					ItemCreation.TargetAccount =
						(await ItemCreation.Accounts.ConfigureAwait(false))
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
		}
	}
}
