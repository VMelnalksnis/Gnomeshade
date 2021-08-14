// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;

using Gnomeshade.Interfaces.WebApi.Models.Accounts;

using JetBrains.Annotations;

#pragma warning disable SA1623
namespace Gnomeshade.Interfaces.WebApi.Models.Importing
{
	/// <summary>
	/// Summary of the report import.
	/// </summary>
	[PublicAPI]
	public sealed record AccountReportResult
	{
		/// <summary>
		/// The user account of the imported report.
		/// </summary>
		public Account UserAccount { get; init; } = null!;

		/// <summary>
		/// The accounts created or referenced during the import.
		/// </summary>
		public List<AccountReference> AccountReferences { get; init; } = new();

		/// <summary>
		/// The products created or referenced during the import.
		/// </summary>
		public List<ProductReference> ProductReferences { get; init; } = new();

		/// <summary>
		/// The transactions created or referenced during the import.
		/// </summary>
		public List<TransactionReference> TransactionReferences { get; init; } = new();
	}
}
