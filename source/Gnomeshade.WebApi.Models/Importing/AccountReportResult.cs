// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;

using Gnomeshade.WebApi.Models.Accounts;

using JetBrains.Annotations;

namespace Gnomeshade.WebApi.Models.Importing;

/// <summary>Summary of the report import.</summary>
[PublicAPI]
public sealed record AccountReportResult
{
	/// <summary>The user account of the imported report.</summary>
	public Account UserAccount { get; set; } = null!;

	/// <summary>The accounts created or referenced during the import.</summary>
	public List<AccountReference> AccountReferences { get; set; } = new();

	/// <summary>The transfers created or referenced during the import.</summary>
	public List<TransferReference> TransferReferences { get; set; } = new();

	/// <summary>The transactions created or referenced during the import.</summary>
	public List<TransactionReference> TransactionReferences { get; set; } = new();
}
