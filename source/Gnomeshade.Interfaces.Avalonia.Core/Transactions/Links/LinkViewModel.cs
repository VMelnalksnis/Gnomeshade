// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.Interfaces.WebApi.Client;

namespace Gnomeshade.Interfaces.Avalonia.Core.Transactions.Links;

/// <summary>Overview of all links for a single transaction.</summary>
public sealed class LinkViewModel : OverviewViewModel<LinkOverview, LinkUpsertionViewModel>
{
	private readonly IGnomeshadeClient _gnomeshadeClient;
	private readonly Guid _transactionId;

	private LinkUpsertionViewModel _details;

	private LinkViewModel(IGnomeshadeClient gnomeshadeClient, Guid transactionId, LinkUpsertionViewModel details)
	{
		_gnomeshadeClient = gnomeshadeClient;
		_transactionId = transactionId;
		_details = details;

		PropertyChanged += OnPropertyChanged;
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

	/// <summary>Initializes a new instance of the <see cref="LinkViewModel"/> class.</summary>
	/// <param name="gnomeshadeClient">A strongly typed API client.</param>
	/// <param name="transactionId">The transaction for which to create a link overview.</param>
	/// <returns>A new instance of the <see cref="LinkViewModel"/> class.</returns>
	public static async Task<LinkViewModel> CreateAsync(IGnomeshadeClient gnomeshadeClient, Guid transactionId)
	{
		var upsertionViewModel = await LinkUpsertionViewModel.CreateAsync(gnomeshadeClient, transactionId).ConfigureAwait(false);
		var viewModel = new LinkViewModel(gnomeshadeClient, transactionId, upsertionViewModel);
		await viewModel.RefreshAsync().ConfigureAwait(false);
		return viewModel;
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

	private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName is nameof(Selected))
		{
			Details = LinkUpsertionViewModel.CreateAsync(_gnomeshadeClient, _transactionId, Selected?.Id).GetAwaiter().GetResult();
		}
	}

	private void DetailsOnUpserted(object? sender, UpsertedEventArgs e)
	{
		Refresh().ConfigureAwait(false).GetAwaiter().GetResult();
	}
}
