// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.Interfaces.WebApi.Models;

namespace Gnomeshade.Interfaces.Avalonia.Core.Transactions.Links;

/// <summary>Overview a single <see cref="Link"/>.</summary>
public sealed class LinkOverview : PropertyChangedBase
{
	/// <summary>Initializes a new instance of the <see cref="LinkOverview"/> class.</summary>
	/// <param name="id">The id of the link.</param>
	/// <param name="uri">The external link value.</param>
	public LinkOverview(Guid id, string uri)
	{
		Id = id;
		Uri = uri;
	}

	/// <summary>Gets the id of the link.</summary>
	public Guid Id { get; }

	/// <summary>Gets the external link value.</summary>
	public string Uri { get; }
}
