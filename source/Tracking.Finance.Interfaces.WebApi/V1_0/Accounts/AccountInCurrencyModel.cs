// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;

using JetBrains.Annotations;

namespace Tracking.Finance.Interfaces.WebApi.V1_0.Accounts
{
	[PublicAPI]
	[SuppressMessage("ReSharper", "SA1623", Justification = "Documentation for public API.")]
	public sealed record AccountInCurrencyModel
	{
		/// <summary>
		/// The id of the account in currency.
		/// </summary>
		public Guid Id { get; init; }

		/// <summary>
		/// The point in time when this account in currency was created.
		/// </summary>
		public DateTimeOffset CreatedAt { get; init; }

		/// <summary>
		/// The id of the owner of this account in currency.
		/// </summary>
		public Guid OwnerId { get; init; }

		/// <summary>
		/// The id of the user which created this account in currency.
		/// </summary>
		public Guid CreatedByUserId { get; init; }

		/// <summary>
		/// The point in time when this account in currency was last modified.
		/// </summary>
		public DateTimeOffset ModifiedAt { get; init; }

		/// <summary>
		/// The id of the user which last modified this account in currency.
		/// </summary>
		public Guid ModifiedByUserId { get; init; }

		/// <summary>
		/// The currency of the account in currency.
		/// </summary>
		public CurrencyModel Currency { get; init; } = null!;
	}
}
