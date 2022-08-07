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
	/// <param name="gnomeshadeClient">A strongly typed API client.</param>
	/// <param name="transactionId">The transaction for which to create a link overview.</param>
	public LinkViewModel(IGnomeshadeClient gnomeshadeClient, Guid transactionId)
	{
		_gnomeshadeClient = gnomeshadeClient;
		_transactionId = transactionId;
		_details = new(gnomeshadeClient, transactionId, null);

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
		Details = new(_gnomeshadeClient, _transactionId, Selected?.Id);
		await Details.RefreshAsync().ConfigureAwait(false);
	}

	/// <inheritdoc />
	protected override async Task Refresh()
	{
		var transaction = await _gnomeshadeClient.GetDetailedTransactionAsync(_transactionId).ConfigureAwait(false);
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
		await _gnomeshadeClient.RemoveLinkFromTransactionAsync(_transactionId, row.Id).ConfigureAwait(false);
		await Refresh().ConfigureAwait(false);
	}

	private async void DetailsOnUpserted(object? sender, UpsertedEventArgs e)
	{
		await RefreshAsync().ConfigureAwait(false);
	}
}
