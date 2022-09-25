// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Gnomeshade.WebApi.Models.Transactions;

using JetBrains.Annotations;

namespace Gnomeshade.WebApi.Models.Importing;

/// <summary>A reference to an transfer that was used during import.</summary>
[PublicAPI]
public sealed record TransferReference
{
	/// <summary>Whether or not the transfer was created during import.</summary>
	public bool Created { get; set; }

	/// <summary>The referenced transfer.</summary>
	public Transfer Transfer { get; set; } = null!;
}
