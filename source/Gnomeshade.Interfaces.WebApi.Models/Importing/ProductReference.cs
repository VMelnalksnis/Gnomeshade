// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Gnomeshade.Interfaces.WebApi.V1_0.Products;

using JetBrains.Annotations;

#pragma warning disable SA1623
namespace Gnomeshade.Interfaces.WebApi.V1_0.Importing.Results
{
	/// <summary>
	/// A reference to a product that was used during an import.
	/// </summary>
	[PublicAPI]
	public sealed record ProductReference
	{
		/// <summary>
		/// Whether or not the product was created during import.
		/// </summary>
		public bool Created { get; init; }

		/// <summary>
		/// The referenced product.
		/// </summary>
		public ProductModel Product { get; init; } = null!;
	}
}
