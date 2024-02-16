// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;

using Avalonia.Controls;

using Gnomeshade.WebApi.Models.Accounts;

using PropertyChanged.SourceGenerator;

namespace Gnomeshade.Avalonia.Core.Accounts;

/// <summary>Values for filtering accounts.</summary>
/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
public sealed partial class AccountFilter(IActivityService activityService) : FilterBase<AccountOverviewRow>(activityService)
{
	/// <summary>Gets or sets a collection of all currencies.</summary>
	[Notify]
	private List<Currency> _currencies = [];

	/// <summary>Gets or sets the text by which to filter account counterparty.</summary>
	[Notify]
	private string? _filterText;

	/// <summary>Gets or sets a value indicating whether to invert <see cref="FilterText"/> matching.</summary>
	[Notify]
	private bool _invertFilterText;

	/// <summary>Gets or sets the currency by which to filter accounts.</summary>
	[Notify]
	private Currency? _selectedCurrency;

	/// <summary>Gets or sets a value indicating whether to invert <see cref="SelectedCurrency"/> matching.</summary>
	[Notify]
	private bool _invertCurrency;

	/// <summary>Gets a delegate for formatting a currency in an <see cref="AutoCompleteBox"/>.</summary>
	public AutoCompleteSelector<object> CurrencySelector => AutoCompleteSelectors.Currency;

	/// <inheritdoc />
	protected override bool FilterRow(AccountOverviewRow row)
	{
		if (FilterText is null && SelectedCurrency is null)
		{
			return true;
		}

		return MatchesFilterText(row) && IsCurrencySelected(row);
	}

	private bool MatchesFilterText(AccountOverviewRow row)
	{
		if (FilterText is null)
		{
			return true;
		}

		var matchesFilter = row.Counterparty.Contains(FilterText, StringComparison.OrdinalIgnoreCase);
		return InvertFilterText ? !matchesFilter : matchesFilter;
	}

	private bool IsCurrencySelected(AccountOverviewRow row)
	{
		if (SelectedCurrency is null)
		{
			return true;
		}

		var matchesFilter = row.Currency == SelectedCurrency.AlphabeticCode;
		return InvertCurrency ? !matchesFilter : matchesFilter;
	}
}
