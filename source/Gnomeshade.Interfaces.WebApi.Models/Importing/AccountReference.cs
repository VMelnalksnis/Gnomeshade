// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Gnomeshade.Interfaces.WebApi.Models.Accounts;

using JetBrains.Annotations;

#pragma warning disable SA1623
namespace Gnomeshade.Interfaces.WebApi.Models.Importing
{
	/// <summary>
	/// A reference to an account that was used during import.
	/// </summary>
	[PublicAPI]
	public sealed record AccountReference
	{
		/// <summary>
		/// Whether or not the account was created during import.
		/// </summary>
		public bool Created { get; init; }

		/// <summary>
		/// The referenced account.
		/// </summary>
		public AccountModel Account { get; init; } = null!;
	}
}
