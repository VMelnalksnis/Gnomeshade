// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using Avalonia.Controls;

using Gnomeshade.WebApi.Models.Accounts;
using Gnomeshade.WebApi.Models.Products;

using NodaTime;
using NodaTime.Extensions;

namespace Gnomeshade.Avalonia.Core.Transactions.Controls;

/// <summary>Values for filtering transactions.</summary>
public sealed class TransactionFilter : FilterBase<TransactionOverview>
{
	private static readonly string[] _isValidNames = { nameof(IsValid) };

	private readonly ZonedClock _clock;
	private readonly IDateTimeZoneProvider _dateTimeZoneProvider;

	private LocalDate? _fromDate;
	private LocalDate? _toDate;
	private Account? _selectedAccount;
	private List<Account> _accounts = new();
	private List<Counterparty> _counterparties = new();
	private Counterparty? _selectedCounterparty;
	private List<Product> _products = new();
	private Product? _selectedProduct;
	private bool _invertAccount;
	private bool _invertCounterparty;
	private bool _invertProduct;
	private bool? _reconciled;
	private bool? _uncategorized;

	/// <summary>Initializes a new instance of the <see cref="TransactionFilter"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	/// <param name="clock">Clock which can provide the current instant.</param>
	/// <param name="dateTimeZoneProvider">Time zone provider for localizing instants to local time.</param>
	public TransactionFilter(IActivityService activityService, IClock clock, IDateTimeZoneProvider dateTimeZoneProvider)
		: base(activityService)
	{
		_dateTimeZoneProvider = dateTimeZoneProvider;
		_clock = clock.InZone(dateTimeZoneProvider.GetSystemDefault());
		SelectCurrentMonth();
	}

	/// <summary>Gets or sets the date from which to get transactions.</summary>
	public LocalDate? FromDate
	{
		get => _fromDate;
		set => SetAndNotifyWithGuard(ref _fromDate, value, nameof(FromDate), _isValidNames);
	}

	/// <summary>Gets or sets the date until which to ge transactions.</summary>
	public LocalDate? ToDate
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

	/// <summary>Gets or sets a value indicating whether to filter reconciled/unreconciled transactions.</summary>
	public bool? Reconciled
	{
		get => _reconciled;
		set => SetAndNotify(ref _reconciled, value);
	}

	/// <summary>Gets or sets a value indicating whether to filter categorized/uncategorized transactions.</summary>
	public bool? Uncategorized
	{
		get => _uncategorized;
		set => SetAndNotify(ref _uncategorized, value);
	}

	internal Interval Interval
	{
		get
		{
			var start = _fromDate?.AtStartOfDayInZone(_dateTimeZoneProvider.GetSystemDefault()).ToInstant();
			var end = _toDate?.AtStartOfDayInZone(_dateTimeZoneProvider.GetSystemDefault()).ToInstant();
			return new(start, end);
		}
	}

	/// <summary>Sets <see cref="FromDate"/> and <see cref="ToDate"/> to the current calendar month.</summary>
	public void SelectCurrentMonth()
	{
		var date = _clock.GetCurrentDate();
		FromDate = new LocalDate(date.Year, date.Month, 1, date.Calendar);

		var lastDayOfMonth = date.Calendar.GetDaysInMonth(date.Year, date.Month);
		ToDate = new LocalDate(date.Year, date.Month, lastDayOfMonth, date.Calendar);
	}

	/// <summary>Sets <see cref="FromDate"/> and <see cref="ToDate"/> to the current calendar year.</summary>
	public void SelectCurrentYear()
	{
		var date = _clock.GetCurrentDate();
		FromDate = new LocalDate(date.Year, 1, 1, date.Calendar);

		var lastMonth = date.Calendar.GetMonthsInYear(date.Year);
		var lastDayOfMonth = date.Calendar.GetDaysInMonth(date.Year, lastMonth);
		ToDate = new LocalDate(date.Year, lastMonth, lastDayOfMonth, date.Calendar);
	}

	/// <inheritdoc />
	protected override bool FilterRow(TransactionOverview overview)
	{
		if (SelectedAccount is null && SelectedCounterparty is null && SelectedProduct is null && Reconciled is null && Uncategorized is null)
		{
			return true;
		}

		return
			IsAccountSelected(overview) &&
			IsCounterpartySelected(overview) &&
			IsProductSelected(overview) &&
			IsReconciled(overview) &&
			IsUncategorized(overview);
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

	private bool IsReconciled(TransactionOverview overview)
	{
		if (Reconciled is not { } reconciled)
		{
			return true;
		}

		return overview.ReconciledAt is not null == reconciled;
	}

	private bool IsUncategorized(TransactionOverview overview)
	{
		if (Uncategorized is not { } uncategorized)
		{
			return true;
		}

		var transferSums = overview.Transfers
			.GroupBy(transfer => transfer.OtherCurrency)
			.Select(grouping => (grouping.Key, grouping.Sum(transfer => transfer.Direction is "→" ? transfer.OtherAmount : -transfer.OtherAmount)))
			.OrderBy(tuple => tuple.Item2)
			.ToList();

		var purchaseSums = overview.Purchases
			.GroupBy(purchase => purchase.CurrencyId)
			.Select(grouping => (grouping.Key, grouping.Sum(purchase => purchase.Price)))
			.OrderBy(tuple => tuple.Item2)
			.ToList();

		var categorized =
			(transferSums.All(sum => sum.Item2 is 0) && purchaseSums.All(sum => sum.Item2 is 0)) ||
			(transferSums.Count == purchaseSums.Count && transferSums.Zip(purchaseSums).All(tuple => Math.Abs(tuple.First.Item2) == Math.Abs(tuple.Second.Item2)));

		return uncategorized ? !categorized : categorized;
	}
}
