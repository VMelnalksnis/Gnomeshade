// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.Interfaces.WebApi.Client;

namespace Gnomeshade.Interfaces.Avalonia.Core.Accounts;

/// <summary>Overview of all accounts.</summary>
public sealed class AccountViewModel : OverviewViewModel<AccountOverviewRow, AccountUpsertionViewModel>
{
	private readonly IGnomeshadeClient _gnomeshadeClient;
	private AccountUpsertionViewModel _details;

	private AccountViewModel(
		IGnomeshadeClient gnomeshadeClient,
		AccountUpsertionViewModel accountUpsertionViewModel)
	{
		_gnomeshadeClient = gnomeshadeClient;
		_details = accountUpsertionViewModel;

		Details.Upserted += DetailsOnUpserted;
		PropertyChanged += OnPropertyChanged;
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

	/// <summary>Asynchronously creates a new instance of the <see cref="AccountViewModel"/> class.</summary>
	/// <param name="gnomeshadeClient">Gnomeshade API client.</param>
	/// <returns>A new instance of <see cref="AccountViewModel"/>.</returns>
	public static async Task<AccountViewModel> CreateAsync(IGnomeshadeClient gnomeshadeClient)
	{
		var upsertionViewModel = await AccountUpsertionViewModel.CreateAsync(gnomeshadeClient).ConfigureAwait(false);
		var viewModel = new AccountViewModel(gnomeshadeClient, upsertionViewModel);
		await viewModel.RefreshAsync().ConfigureAwait(false);
		return viewModel;
	}

	/// <inheritdoc />
	public override async Task RefreshAsync()
	{
		var counterparties = await _gnomeshadeClient.GetCounterpartiesAsync().ConfigureAwait(false);
		var accounts = await _gnomeshadeClient.GetAccountsAsync().ConfigureAwait(false);
		var accountOverviewRows = accounts.Translate(counterparties).ToList();

		var selected = Selected;
		var sort = DataGridView.SortDescriptions;
		Rows = new(accountOverviewRows);

		var group = new DataGridTypedGroupDescription<AccountOverviewRow, string>(row => row.Counterparty);
		DataGridView.GroupDescriptions.Add(group);
		DataGridView.SortDescriptions.AddRange(sort);
		Selected = Rows.SingleOrDefault(overview => overview.Id == selected?.Id);
	}

	/// <inheritdoc />
	protected override Task DeleteAsync(AccountOverviewRow row) => throw new NotImplementedException();

	private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName is nameof(Selected))
		{
			Details = AccountUpsertionViewModel
				.CreateAsync(_gnomeshadeClient, Selected?.Id)
				.ConfigureAwait(false)
				.GetAwaiter()
				.GetResult();
		}
	}

	private void DetailsOnUpserted(object? sender, UpsertedEventArgs e)
	{
		RefreshAsync().ConfigureAwait(false).GetAwaiter().GetResult();
	}
}
