// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.Interfaces.WebApi.Client;

namespace Gnomeshade.Interfaces.Avalonia.Core.Counterparties;

/// <summary>List of all counterparties.</summary>
public sealed class CounterpartyViewModel : OverviewViewModel<CounterpartyRow, CounterpartyUpsertionViewModel>
{
	private readonly IGnomeshadeClient _gnomeshadeClient;

	private CounterpartyUpsertionViewModel _details;

	/// <summary>Initializes a new instance of the <see cref="CounterpartyViewModel"/> class.</summary>
	/// <param name="gnomeshadeClient">Gnomeshade API client.</param>
	public CounterpartyViewModel(IGnomeshadeClient gnomeshadeClient)
	{
		_gnomeshadeClient = gnomeshadeClient;
		_details = new(gnomeshadeClient, null);

		_details.Upserted += DetailsOnUpserted;
	}

	/// <inheritdoc />
	public override CounterpartyUpsertionViewModel Details
	{
		get => _details;
		set
		{
			Details.Upserted -= DetailsOnUpserted;
			SetAndNotify(ref _details, value);
			Details.Upserted += DetailsOnUpserted;
		}
	}

	/// <inheritdoc />
	public override Task UpdateSelection()
	{
		Details = new(_gnomeshadeClient, Selected?.Id);
		return Details.RefreshAsync();
	}

	/// <inheritdoc />
	protected override async Task Refresh()
	{
		var counterparties = await _gnomeshadeClient.GetCounterpartiesAsync().ConfigureAwait(false);
		var userCounterparty = await _gnomeshadeClient.GetMyCounterpartyAsync().ConfigureAwait(false);

		var counterpartyRows = counterparties.Select(counterparty => new CounterpartyRow(counterparty, 0)).ToList();
		Rows = new(counterpartyRows);

		foreach (var row in Rows)
		{
			var loans = await _gnomeshadeClient.GetCounterpartyLoansAsync(row.Id).ConfigureAwait(false);
			var issued = loans
				.Where(loan =>
					loan.IssuingCounterpartyId == row.Id &&
					loan.ReceivingCounterpartyId == userCounterparty.Id)
				.Sum(loan => loan.Amount);

			var received = loans
				.Where(loan =>
					loan.IssuingCounterpartyId == userCounterparty.Id &&
					loan.ReceivingCounterpartyId == row.Id)
				.Sum(loan => loan.Amount);

			row.LoanBalance = issued - received;
		}
	}

	/// <inheritdoc />
	protected override Task DeleteAsync(CounterpartyRow row)
	{
		const string message = "Deleting counterparties is not yet supported";
		throw new NotSupportedException(message);
	}

	private async void DetailsOnUpserted(object? sender, UpsertedEventArgs e)
	{
		await RefreshAsync();
	}
}
