// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Threading.Tasks;

using Avalonia.Controls;

using Gnomeshade.Interfaces.Desktop.Views;
using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models.Accounts;
using Gnomeshade.Interfaces.WebApi.Models.Products;

namespace Gnomeshade.Interfaces.Desktop.ViewModels
{
	/// <summary>
	/// Form for creating a single new transaction item.
	/// </summary>
	public class TransactionItemCreationViewModel : ViewModelBase<TransactionItemCreationView>
	{
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

		private TransactionItemCreationViewModel(
			List<Account> accounts,
			List<Currency> currencies,
			List<Product> products)
		{
			Accounts = accounts;
			Currencies = currencies;
			Products = products;

			AccountSelector = (_, item) => ((Account)item).Name;
			CurrencySelector = (_, item) => ((Currency)item).AlphabeticCode;
			ProductSelector = (_, item) => ((Product)item).Name;
		}

		/// <summary>
		/// Gets a collection of all active accounts.
		/// </summary>
		public List<Account> Accounts { get; }

		public AutoCompleteSelector<object> AccountSelector { get; }

		/// <summary>
		/// Gets a collection of all currencies.
		/// </summary>
		public List<Currency> Currencies { get; }

		public AutoCompleteSelector<object> CurrencySelector { get; }

		/// <summary>
		/// Gets a collection of all products.
		/// </summary>
		public List<Product> Products { get; }

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
		/// Asynchronously creates a new instance of the <see cref="TransactionItemCreationViewModel"/> class.
		/// </summary>
		/// <param name="gnomeshadeClient">Gnomeshade API client.</param>
		/// <returns>A new instance of the <see cref="TransactionItemCreationViewModel"/> class.</returns>
		public static async Task<TransactionItemCreationViewModel> CreateAsync(IGnomeshadeClient gnomeshadeClient)
		{
			var accounts = await gnomeshadeClient.GetActiveAccountsAsync();
			var currencies = await gnomeshadeClient.GetCurrenciesAsync();
			var products = await gnomeshadeClient.GetProductsAsync();
			return new(accounts, currencies, products);
		}
	}
}
