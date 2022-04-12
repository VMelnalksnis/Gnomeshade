﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading.Tasks;

using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models;

namespace Gnomeshade.Interfaces.Avalonia.Core.Transactions.Links;

/// <summary>Create or update <see cref="Link"/>.</summary>
public sealed class LinkUpsertionViewModel : UpsertionViewModel
{
	private readonly Guid _transactionId;

	private Guid? _id;
	private string? _uriValue;

	private LinkUpsertionViewModel(IGnomeshadeClient gnomeshadeClient, Guid transactionId)
		: base(gnomeshadeClient)
	{
		_transactionId = transactionId;
	}

	private LinkUpsertionViewModel(IGnomeshadeClient gnomeshadeClient, Guid transactionId, Link link)
		: this(gnomeshadeClient, transactionId)
	{
		_id = link.Id;
		UriValue = link.Uri;
	}

	/// <summary>Gets or sets the link value.</summary>
	public string? UriValue
	{
		get => _uriValue;
		set => SetAndNotifyWithGuard(ref _uriValue, value, nameof(UriValue), CanSaveNames);
	}

	/// <inheritdoc />
	public override bool CanSave => Uri.IsWellFormedUriString(UriValue, UriKind.Absolute);

	/// <summary>Initializes a new instance of the <see cref="LinkUpsertionViewModel"/> class.</summary>
	/// <param name="gnomeshadeClient">The strongly typed API client.</param>
	/// <param name="transactionId">The id of the transaction to which to add the link.</param>
	/// <param name="id">The id of the link to update.</param>
	/// <returns>A new instance of the <see cref="LinkUpsertionViewModel"/> class.</returns>
	public static async Task<LinkUpsertionViewModel> CreateAsync(
		IGnomeshadeClient gnomeshadeClient,
		Guid transactionId,
		Guid? id = null)
	{
		if (id is null)
		{
			return new(gnomeshadeClient, transactionId);
		}

		var link = await gnomeshadeClient.GetLinkAsync(id.Value).ConfigureAwait(false);
		return new(gnomeshadeClient, transactionId, link);
	}

	/// <inheritdoc />
	protected override async Task<Guid> SaveValidatedAsync()
	{
		var linkCreation = new LinkCreation { Uri = new(UriValue!) };
		_id ??= Guid.NewGuid();
		await GnomeshadeClient.PutLinkAsync(_id.Value, linkCreation).ConfigureAwait(false);
		await GnomeshadeClient.AddLinkToTransactionAsync(_transactionId, _id.Value).ConfigureAwait(false);
		return _id.Value;
	}
}