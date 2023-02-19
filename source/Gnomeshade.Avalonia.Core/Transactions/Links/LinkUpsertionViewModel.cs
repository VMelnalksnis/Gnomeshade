// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models;

using PropertyChanged.SourceGenerator;

using static System.StringComparer;

namespace Gnomeshade.Avalonia.Core.Transactions.Links;

/// <summary>Create or update <see cref="Link"/>.</summary>
public sealed partial class LinkUpsertionViewModel : UpsertionViewModel
{
	private readonly Guid _transactionId;

	/// <summary>Gets or sets the link value.</summary>
	[Notify]
	private string? _uriValue;

	/// <summary>Initializes a new instance of the <see cref="LinkUpsertionViewModel"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	/// <param name="gnomeshadeClient">The strongly typed API client.</param>
	/// <param name="transactionId">The id of the transaction to which to add the link.</param>
	/// <param name="id">The id of the link to update.</param>
	public LinkUpsertionViewModel(
		IActivityService activityService,
		IGnomeshadeClient gnomeshadeClient,
		Guid transactionId,
		Guid? id)
		: base(activityService, gnomeshadeClient)
	{
		_transactionId = transactionId;
		Id = id;
	}

	/// <inheritdoc />
	public override bool CanSave => Uri.IsWellFormedUriString(UriValue, UriKind.Absolute);

	/// <inheritdoc />
	protected override async Task Refresh()
	{
		if (Id is not { } linkId)
		{
			return;
		}

		var link = await GnomeshadeClient.GetLinkAsync(linkId);
		UriValue = link.Uri;
	}

	/// <inheritdoc />
	protected override async Task<Guid> SaveValidatedAsync()
	{
		var links = await GnomeshadeClient.GetLinksAsync();
		var existingLink = links.SingleOrDefault(link => InvariantCultureIgnoreCase.Equals(link.Uri, UriValue));

		var id = existingLink?.Id ?? Id ?? Guid.NewGuid();
		if (existingLink is null)
		{
			var linkCreation = new LinkCreation { Uri = new(UriValue!) };
			await GnomeshadeClient.PutLinkAsync(id, linkCreation);
		}

		await GnomeshadeClient.AddLinkToTransactionAsync(_transactionId, id);
		return id;
	}
}
