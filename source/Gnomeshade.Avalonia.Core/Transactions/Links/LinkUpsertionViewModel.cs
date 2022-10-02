// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models;

namespace Gnomeshade.Avalonia.Core.Transactions.Links;

/// <summary>Create or update <see cref="Link"/>.</summary>
public sealed class LinkUpsertionViewModel : UpsertionViewModel
{
	private readonly Guid _transactionId;

	private Guid? _id;
	private string? _uriValue;

	/// <summary>Initializes a new instance of the <see cref="LinkUpsertionViewModel"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	/// <param name="gnomeshadeClient">The strongly typed API client.</param>
	/// <param name="transactionId">The id of the transaction to which to add the link.</param>
	/// <param name="id">The id of the link to update.</param>
	public LinkUpsertionViewModel(IActivityService activityService, IGnomeshadeClient gnomeshadeClient, Guid transactionId, Guid? id)
		: base(activityService, gnomeshadeClient)
	{
		_transactionId = transactionId;
		_id = id;
	}

	/// <summary>Gets or sets the link value.</summary>
	public string? UriValue
	{
		get => _uriValue;
		set => SetAndNotifyWithGuard(ref _uriValue, value, nameof(UriValue), CanSaveNames);
	}

	/// <inheritdoc />
	public override bool CanSave => Uri.IsWellFormedUriString(UriValue, UriKind.Absolute);

	/// <inheritdoc />
	protected override async Task Refresh()
	{
		if (_id is null)
		{
			return;
		}

		var link = await GnomeshadeClient.GetLinkAsync(_id.Value);
		UriValue = link.Uri;
	}

	/// <inheritdoc />
	protected override async Task<Guid> SaveValidatedAsync()
	{
		var links = await GnomeshadeClient.GetLinksAsync();
		var existingLink = links.SingleOrDefault(link => StringComparer.InvariantCultureIgnoreCase.Equals(link.Uri, UriValue));
		if (existingLink is not null)
		{
			_id = existingLink.Id;
		}
		else
		{
			var linkCreation = new LinkCreation { Uri = new(UriValue!) };
			_id ??= Guid.NewGuid();
			await GnomeshadeClient.PutLinkAsync(_id.Value, linkCreation);
		}

		await GnomeshadeClient.AddLinkToTransactionAsync(_transactionId, _id.Value);
		return _id.Value;
	}
}
