// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Collections;

using Gnomeshade.WebApi.Client;

namespace Gnomeshade.Avalonia.Core.Accounts;

/// <summary>Overview of all accounts.</summary>
public sealed class AccountViewModel : OverviewViewModel<AccountOverviewRow, AccountUpsertionViewModel>
{
	private static readonly DataGridSortDescription[] _sortDescriptions =
	[
		new DataGridComparerSortDescription(new AccountOverviewComparer(), ListSortDirection.Ascending),
	];

	private readonly IGnomeshadeClient _gnomeshadeClient;
	private AccountUpsertionViewModel _details;

	/// <summary>Initializes a new instance of the <see cref="AccountViewModel"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	/// <param name="gnomeshadeClient">Gnomeshade API client.</param>
	public AccountViewModel(IActivityService activityService, IGnomeshadeClient gnomeshadeClient)
		: base(activityService)
	{
		_gnomeshadeClient = gnomeshadeClient;
		_details = new(activityService, _gnomeshadeClient, null);

		DataGridView.SortDescriptions.AddRange(_sortDescriptions);

		Filter = new(activityService);

		Filter.PropertyChanged += FilterOnPropertyChanged;
		Details.Upserted += DetailsOnUpserted;
	}

	/// <inheritdoc />
	public override AccountUpsertionViewModel Details
	{
		get => _details;
		set
		{
			Details.Upserted -= DetailsOnUpserted;
			SetAndNotify(ref _details, value);
			Details.Upserted += DetailsOnUpserted;
		}
	}

	/// <summary>Gets the account filter.</summary>
	public AccountFilter Filter { get; }

	/// <inheritdoc />
	public override Task UpdateSelection()
	{
		Details = new(ActivityService, _gnomeshadeClient, Selected?.Id);
		return Details.RefreshAsync();
	}

	/// <inheritdoc />
	protected override async Task Refresh()
	{
		var counterparties = await _gnomeshadeClient.GetCounterpartiesAsync();
		var accounts = await _gnomeshadeClient.GetAccountsAsync();
		var accountOverviewRows = accounts.Translate(counterparties).ToList();

		var selected = Selected;
		var sort = DataGridView.SortDescriptions;
		Rows = new(accountOverviewRows);

		var currencies = await _gnomeshadeClient.GetCurrenciesAsync();
		Filter.Currencies = currencies;

		var group = new DataGridTypedGroupDescription<AccountOverviewRow, string>(row => row.Counterparty);
		DataGridView.GroupDescriptions.Add(group);
		DataGridView.SortDescriptions.AddRange(sort);
		DataGridView.Filter = Filter.Filter;

		Selected = Rows.SingleOrDefault(overview => overview.Id == selected?.Id && overview.InCurrencyId == selected.InCurrencyId);

		if (Details.Counterparties.Count is 0)
		{
			await Details.RefreshAsync();
		}

		foreach (var row in Rows.ToList())
		{
			var balances = await _gnomeshadeClient.GetAccountBalanceAsync(row.Id);
			var balance = balances.SingleOrDefault(balance => balance.AccountInCurrencyId == row.InCurrencyId);
			var sum = balance is null ? 0 : balance.TargetAmount - balance.SourceAmount;
			row.Balance = sum;
		}
	}

	/// <inheritdoc />
	protected override Task DeleteAsync(AccountOverviewRow row) => throw new NotImplementedException();

	private async void DetailsOnUpserted(object? sender, UpsertedEventArgs e)
	{
		await RefreshAsync();
	}

	private void FilterOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName is not nameof(AccountFilter.Currencies))
		{
			DataGridView.Refresh();
		}
	}
}
