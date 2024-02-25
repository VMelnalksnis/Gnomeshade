// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using Avalonia.Controls;

using Gnomeshade.Avalonia.Core.Products;
using Gnomeshade.WebApi.Models.Accounts;
using Gnomeshade.WebApi.Models.Loans;
using Gnomeshade.WebApi.Models.Products;

using NodaTime;
using NodaTime.Extensions;

using PropertyChanged.SourceGenerator;

namespace Gnomeshade.Avalonia.Core.Transactions.Controls;

/// <summary>Values for filtering transactions.</summary>
public sealed partial class TransactionFilter : FilterBase<TransactionOverview>
{
	private readonly ZonedClock _clock;
	private readonly IDateTimeZoneProvider _dateTimeZoneProvider;

	/// <summary>Gets or sets the date from which to get transactions.</summary>
	[Notify]
	private LocalDate? _fromDate;

	/// <summary>Gets or sets the date until which to ge transactions.</summary>
	[Notify]
	private LocalDate? _toDate;

	/// <summary>Gets or sets the selected account from <see cref="Accounts"/>.</summary>
	[Notify]
	private Account? _selectedAccount;

	/// <summary>Gets or sets a collection of all active accounts.</summary>
	[Notify(Setter.Internal)]
	private List<Account> _accounts = [];

	/// <summary>Gets or sets a collection of all counterparties.</summary>
	[Notify(Setter.Internal)]
	private List<Counterparty> _counterparties = [];

	/// <summary>Gets or sets the selected counterparty from <see cref="Counterparties"/>.</summary>
	[Notify]
	private Counterparty? _selectedCounterparty;

	/// <summary>Gets or sets a collection of all products.</summary>
	[Notify(Setter.Internal)]
	private List<Product> _products = [];

	/// <summary>Gets or sets the selected product from <see cref="Products"/>.</summary>
	[Notify]
	private Product? _selectedProduct;

	/// <summary>Gets or sets a collection of all categories.</summary>
	[Notify(Setter.Internal)]
	private List<Category> _categories = [];

	/// <summary>Gets or sets the selected category from <see cref="Categories"/>.</summary>
	[Notify]
	private Category? _selectedCategory;

	/// <summary>Gets or sets a collection of all loans.</summary>
	[Notify(Setter.Internal)]
	private List<Loan> _loans = [];

	/// <summary>Gets or sets the selected loan from <see cref="Loans"/>.</summary>
	[Notify]
	private Loan? _selectedLoan;

	/// <summary>Gets or sets a value indicating whether to filter transactions with or without <see cref="SelectedAccount"/>.</summary>
	[Notify]
	private bool _invertAccount;

	/// <summary>Gets or sets a value indicating whether to filter transactions with or without <see cref="SelectedCounterparty"/>.</summary>
	[Notify]
	private bool _invertCounterparty;

	/// <summary>Gets or sets a value indicating whether to filter transactions with or without <see cref="SelectedProduct"/>.</summary>
	[Notify]
	private bool _invertProduct;

	/// <summary>Gets or sets a value indicating whether to filter transactions with or without <see cref="SelectedCategory"/>.</summary>
	[Notify]
	private bool _invertCategory;

	/// <summary>Gets or sets a value indicating whether to filter transactions with or without <see cref="SelectedLoan"/>.</summary>
	[Notify]
	private bool _invertLoan;

	/// <summary>Gets or sets a value indicating whether to filter reconciled/unreconciled transactions.</summary>
	[Notify]
	private bool? _reconciled;

	/// <summary>Gets or sets a value indicating whether to filter categorized/uncategorized transactions.</summary>
	[Notify]
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

	/// <summary>Gets a value indicating whether the current values are valid search parameters.</summary>
	public bool IsValid => ToDate is null || FromDate is null || ToDate >= FromDate;

	/// <inheritdoc cref="AutoCompleteSelectors.Account"/>
	public AutoCompleteSelector<object> AccountSelector => AutoCompleteSelectors.Account;

	/// <inheritdoc cref="AutoCompleteSelectors.Counterparty"/>
	public AutoCompleteSelector<object> CounterpartySelector => AutoCompleteSelectors.Counterparty;

	/// <inheritdoc cref="AutoCompleteSelectors.Product"/>
	public AutoCompleteSelector<object> ProductSelector => AutoCompleteSelectors.Product;

	/// <inheritdoc cref="AutoCompleteSelectors.Category"/>
	public AutoCompleteSelector<object> CategorySelector => AutoCompleteSelectors.Category;

	/// <inheritdoc cref="AutoCompleteSelectors.Loan"/>
	public AutoCompleteSelector<object> LoanSelector => AutoCompleteSelectors.Loan;

	internal Interval Interval
	{
		get
		{
			var start = FromDate?.AtStartOfDayInZone(_dateTimeZoneProvider.GetSystemDefault()).ToInstant();
			var end = ToDate?.AtStartOfDayInZone(_dateTimeZoneProvider.GetSystemDefault()).ToInstant();
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

	/// <summary>Extends the selected interval one month into the past.</summary>
	public void ExtendOneMonthBack()
	{
		FromDate = FromDate?.PlusMonths(-1);
	}

	/// <summary>Moves the selected interval one month into the past.</summary>
	public void MoveOneMonthBack()
	{
		FromDate = FromDate?.PlusMonths(-1);
		ToDate = ToDate?.PlusMonths(-1);
	}

	/// <summary>Extends the selected interval one month into the future.</summary>
	public void ExtendOneMonthForward()
	{
		ToDate = ToDate?.PlusMonths(1);
	}

	/// <summary>Moves the selected interval one month into the future.</summary>
	public void MoveOneMonthForward()
	{
		FromDate = FromDate?.PlusMonths(1);
		ToDate = ToDate?.PlusMonths(1);
	}

	/// <inheritdoc />
	protected override bool FilterRow(TransactionOverview overview)
	{
		if (SelectedAccount is null && SelectedCounterparty is null && SelectedProduct is null && SelectedCategory is null && SelectedLoan is null && Reconciled is null && Uncategorized is null)
		{
			return true;
		}

		return
			IsAccountSelected(overview) &&
			IsCounterpartySelected(overview) &&
			IsProductSelected(overview) &&
			IsCategorySelected(overview) &&
			IsLoanSelected(overview) &&
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

	private bool IsCategorySelected(TransactionOverview overview)
	{
		if (SelectedCategory is null)
		{
			return true;
		}

		var categoryNodes = overview
			.Purchases
			.Select(purchase => Products.Single(product => product.Id == purchase.ProductId))
			.Where(product => product.CategoryId is not null)
			.Select(product => Categories.Single(category => category.Id == product.CategoryId))
			.Select(category => CategoryNode.FromCategory(category, Categories));

		return InvertCategory
			? categoryNodes.All(node => !node.Contains(SelectedCategory.Id))
			: categoryNodes.Any(node => node.Contains(SelectedCategory.Id));
	}

	private bool IsLoanSelected(TransactionOverview overview)
	{
		if (SelectedLoan is null)
		{
			return true;
		}

		return InvertLoan
			? overview.LoanPayments.All(payment => payment.LoanId != SelectedLoan.Id)
			: overview.LoanPayments.Any(payment => payment.LoanId == SelectedLoan.Id);
	}

	private bool IsReconciled(TransactionOverview overview)
	{
		if (Reconciled is not { } reconciled)
		{
			return true;
		}

		return overview.Reconciled == reconciled;
	}

	private bool IsUncategorized(TransactionOverview overview)
	{
		if (Uncategorized is not { } uncategorized)
		{
			return true;
		}

		var transferSums = overview.Transfers
			.GroupBy(transfer => transfer.OtherCurrency)
			.Select(grouping => (grouping.Key,
				grouping.Sum(transfer => transfer.Direction is "→" ? transfer.OtherAmount : -transfer.OtherAmount)))
			.OrderBy(tuple => tuple.Item2)
			.ToList();

		var purchaseSums = overview.Purchases
			.GroupBy(purchase => purchase.CurrencyId)
			.Select(grouping => (grouping.Key, grouping.Sum(purchase => purchase.Price)))
			.OrderBy(tuple => tuple.Item2)
			.ToList();

		var categorized =
			(transferSums.All(sum => sum.Item2 is 0) && purchaseSums.All(sum => sum.Item2 is 0)) ||
			(transferSums.Count == purchaseSums.Count &&
			transferSums.Zip(purchaseSums).All(tuple => Math.Abs(tuple.First.Item2) == Math.Abs(tuple.Second.Item2)));

		return uncategorized ? !categorized : categorized;
	}
}
