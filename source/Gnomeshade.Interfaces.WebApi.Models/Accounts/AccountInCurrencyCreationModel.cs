// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

using JetBrains.Annotations;

namespace Gnomeshade.Interfaces.WebApi.Models.Accounts
{
	/// <summary>
	/// The information needed to add a currency to an account.
	/// </summary>
	[PublicAPI]
	[SuppressMessage("ReSharper", "SA1623", Justification = "Documentation for public API.")]
	public sealed record AccountInCurrencyCreationModel
	{
		/// <summary>
		/// The currency to add to an account.
		/// </summary>
		[Required]
		public Guid? CurrencyId { get; init; }
	}
}
