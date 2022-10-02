// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.WebApi.Client;

namespace Gnomeshade.Avalonia.Core.Transactions.Links;

/// <summary>Overview of all links for a single transaction.</summary>
public sealed class LinkViewModel : OverviewViewModel<LinkOverview, LinkUpsertionViewModel>
{
	private readonly IGnomeshadeClient _gnomeshadeClient;
	private readonly Guid _transactionId;

	private LinkUpsertionViewModel _details;

	/// <summary>Initializes a new instance of the <see cref="LinkViewModel"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	/// <param name="gnomeshadeClient">A strongly typed API client.</param>
	/// <param name="transactionId">The transaction for which to create a link overview.</param>
	public LinkViewModel(IActivityService activityService, IGnomeshadeClient gnomeshadeClient, Guid transactionId)
		: base(activityService)
	{
		_gnomeshadeClient = gnomeshadeClient;
		_transactionId = transactionId;
		_details = new(activityService, gnomeshadeClient, transactionId, null);

		_details.Upserted += DetailsOnUpserted;
	}

	/// <inheritdoc />
	public override LinkUpsertionViewModel Details
	{
		get => _details;
		set
		{
			_details.Upserted -= DetailsOnUpserted;
			SetAndNotify(ref _details, value);
			_details.Upserted += DetailsOnUpserted;
		}
	}

	/// <inheritdoc />
	public override async Task UpdateSelection()
	{
		Details = new(ActivityService, _gnomeshadeClient, _transactionId, Selected?.Id);
		await Details.RefreshAsync();
	}

	/// <summary>Imports purchases from the <see cref="OverviewViewModel{TRow,TUpsertion}.Selected"/> link.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task ImportSelected()
	{
		if (Selected is null)
		{
			return;
		}

		await _gnomeshadeClient.AddPurchasesFromDocument(_transactionId, Selected.Id);
		await RefreshAsync();
	}

	/// <inheritdoc />
	protected override async Task Refresh()
	{
		var transaction = await _gnomeshadeClient.GetDetailedTransactionAsync(_transactionId);
		var overviews = transaction.Links
			.OrderBy(link => link.CreatedAt)
			.Select(link => new LinkOverview(link.Id, link.Uri)).ToList();

		var selected = Selected;

		IsReadOnly = transaction.Reconciled;
		Rows = new(overviews);
		Selected = Rows.SingleOrDefault(overview => overview.Id == selected?.Id);
	}

	/// <inheritdoc />
	protected override async Task DeleteAsync(LinkOverview row)
	{
		await _gnomeshadeClient.RemoveLinkFromTransactionAsync(_transactionId, row.Id);
		await RefreshAsync();
	}

	private async void DetailsOnUpserted(object? sender, UpsertedEventArgs e)
	{
		await RefreshAsync();
	}
}
