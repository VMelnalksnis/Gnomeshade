// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;

using JetBrains.Annotations;

namespace Gnomeshade.Interfaces.WebApi.V1_0.Importing
{
	/// <summary>
	/// Details about the result of importing a transaction.
	/// </summary>
	[PublicAPI]
	[SuppressMessage("ReSharper", "SA1623", Justification = "Documentation for public API.")]
	public sealed record TransactionImportResult(bool Created, Guid Id)
	{
		/// <summary>
		/// A value indicating whether or not a new transaction was created.
		/// </summary>
		public bool Created { get; init; } = Created;

		/// <summary>
		/// The id of the respective transaction.
		/// </summary>
		public Guid Id { get; init; } = Id;
	}
}
