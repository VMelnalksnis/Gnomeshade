﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Gnomeshade.Interfaces.WebApi.V1_0.Transactions;

using JetBrains.Annotations;

#pragma warning disable SA1623
namespace Gnomeshade.Interfaces.WebApi.V1_0.Importing.Results
{
	/// <summary>
	/// A reference to an transaction that was used during import.
	/// </summary>
	[PublicAPI]
	public sealed record TransactionReference
	{
		/// <summary>
		/// Whether or not the transaction was created during import.
		/// </summary>
		public bool Created { get; init; }

		/// <summary>
		/// The referenced transaction.
		/// </summary>
		public TransactionModel Transaction { get; init; } = null!;
	}
}
