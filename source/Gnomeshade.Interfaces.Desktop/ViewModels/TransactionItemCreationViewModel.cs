// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Threading.Tasks;

using Avalonia.Controls;

using Gnomeshade.Interfaces.Desktop.Views;
using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.V1_0.Accounts;
using Gnomeshade.Interfaces.WebApi.V1_0.Products;

namespace Gnomeshade.Interfaces.Desktop.ViewModels
{
	public class TransactionItemCreationViewModel : ViewModelBase<TransactionItemCreationView>
	{
		private readonly IFinanceClient _financeClient;

		private AccountModel? _sourceAccount;
		private decimal? _sourceAmount;
		private CurrencyModel? _sourceCurrency;
		private AccountModel? _targetAccount;
		private decimal? _targetAmount;
		private CurrencyModel? _targetCurrency;
		private ProductModel? _product;
		private decimal? _amount;
		private string? _bankReference;
		private string? _externalReference;
		private string? _internalReference;

		/// <summary>
		/// Initializes a new instance of the <see cref="TransactionItemCreationViewModel"/> class.
		/// </summary>
		public TransactionItemCreationViewModel()
			: this(new FinanceClient())
		{
		}

		public TransactionItemCreationViewModel(IFinanceClient financeClient)
		{
			_financeClient = financeClient;

			Accounts = GetAccountsAsync();
			Currencies = GetCurrenciesAsync();
			Products = GetProductsAsync();

			AccountSelector = (_, item) => ((AccountModel)item).Name;
			CurrencySelector = (_, item) => ((CurrencyModel)item).AlphabeticCode;
			ProductSelector = (_, item) => ((ProductModel)item).Name;
		}

		/// <summary>
		/// Gets a collection of all accounts.
		/// </summary>
		public Task<List<AccountModel>> Accounts { get; }

		public AutoCompleteSelector<object> AccountSelector { get; }

		/// <summary>
		/// Gets a collection of all currencies.
		/// </summary>
		public Task<List<CurrencyModel>> Currencies { get; }

		public AutoCompleteSelector<object> CurrencySelector { get; }

		public Task<List<ProductModel>> Products { get; }

		public AutoCompleteSelector<object> ProductSelector { get; }

		/// <summary>
		/// Gets or sets the source account of the transaction item.
		/// </summary>
		public AccountModel? SourceAccount
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
		public CurrencyModel? SourceCurrency
		{
			get => _sourceCurrency;
			set => SetAndNotifyWithGuard(ref _sourceCurrency, value, nameof(SourceCurrency), nameof(CanCreate), nameof(IsTargetAmountReadOnly));
		}

		/// <summary>
		/// Gets or sets the target account of the transaction item.
		/// </summary>
		public AccountModel? TargetAccount
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

		public bool IsTargetAmountReadOnly => SourceCurrency == TargetCurrency;

		/// <summary>
		/// Gets or sets the currency of <see cref="TargetAmount"/>.
		/// </summary>
		public CurrencyModel? TargetCurrency
		{
			get => _targetCurrency;
			set => SetAndNotifyWithGuard(ref _targetCurrency, value, nameof(TargetCurrency), nameof(CanCreate), nameof(IsTargetAmountReadOnly));
		}

		public ProductModel? Product
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

		public string? BankReference
		{
			get => _bankReference;
			set => SetAndNotify(ref _bankReference, value, nameof(BankReference));
		}

		public string? ExternalReference
		{
			get => _externalReference;
			set => SetAndNotify(ref _externalReference, value, nameof(ExternalReference));
		}

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

		private async Task<List<AccountModel>> GetAccountsAsync()
		{
			return await _financeClient.GetAccountsAsync().ConfigureAwait(false);
		}

		private async Task<List<CurrencyModel>> GetCurrenciesAsync()
		{
			return await _financeClient.GetCurrenciesAsync().ConfigureAwait(false);
		}

		private async Task<List<ProductModel>> GetProductsAsync()
		{
			return await _financeClient.GetProductsAsync().ConfigureAwait(false);
		}
	}
}
