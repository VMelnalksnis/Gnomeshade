// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using JetBrains.Annotations;

#pragma warning disable SA1623
namespace Gnomeshade.Interfaces.WebApi.Models.Accounts
{
	[PublicAPI]
	public sealed record CounterpartyModel
	{
		/// <summary>
		/// The id of the counterparty.
		/// </summary>
		public Guid Id { get; init; }

		/// <summary>
		/// The point in time when this counterparty was created.
		/// </summary>
		public DateTimeOffset CreatedAt { get; init; }

		/// <summary>
		/// The id of the owner of this counterparty.
		/// </summary>
		public Guid OwnerId { get; init; } // todo is this relevant?

		/// <summary>
		/// The id of the user which created this counterparty.
		/// </summary>
		public Guid CreatedByUserId { get; init; }

		/// <summary>
		/// The point in time when this counterparty was last modified.
		/// </summary>
		public DateTimeOffset ModifiedAt { get; init; }

		/// <summary>
		/// The id of the user which last modified this counterparty.
		/// </summary>
		public Guid ModifiedByUserId { get; init; }

		/// <summary>
		/// The name of the counterparty.
		/// </summary>
		public string Name { get; init; } = null!;
	}
}
