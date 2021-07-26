// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

using JetBrains.Annotations;

namespace Gnomeshade.Interfaces.WebApi.V1_0.Accounts
{
	/// <summary>
	/// The information needed in order to create a new account.
	/// </summary>
	[PublicAPI]
	[SuppressMessage("ReSharper", "SA1623", Justification = "Documentation for public API.")]
	public sealed record AccountCreationModel
	{
		/// <summary>
		/// The name of the account to create. Required.
		/// </summary>
		[Required]
		public string? Name { get; init; }

		/// <summary>
		/// The id of the preferred currency of the account to create. Required.
		/// </summary>
		[Required]
		public Guid? PreferredCurrencyId { get; init; }

		/// <summary>
		/// The BIC (Business Identifier Code) of the account to create.
		/// </summary>
		public string? Bic { get; init; }

		/// <summary>
		/// The IBAN (International Bank Account Number) of the account to create.
		/// </summary>
		public string? Iban { get; init; }

		/// <summary>
		/// The account number of the account to create.
		/// </summary>
		public string? AccountNumber { get; init; }

		/// <summary>
		/// A collection of currencies available for the account to create. Requires at least one currency.
		/// </summary>
		[Required]
		[MinLength(1)]
		public List<AccountInCurrencyCreationModel>? Currencies { get; init; }
	}
}
