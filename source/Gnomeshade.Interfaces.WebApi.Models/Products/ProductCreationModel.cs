// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

using JetBrains.Annotations;

namespace Gnomeshade.Interfaces.WebApi.Models.Products
{
	/// <summary>
	/// The information needed to create or update a product.
	/// </summary>
	[PublicAPI]
	[SuppressMessage("ReSharper", "SA1623", Justification = "Documentation for public API.")]
	public sealed record ProductCreationModel
	{
		/// <summary>
		/// The id of the product to update.
		/// </summary>
		public Guid? Id { get; init; }

		/// <summary>
		/// The name of the product.
		/// </summary>
		[Required(AllowEmptyStrings = false)]
		public string? Name { get; init; }

		/// <summary>
		/// The description of the product.
		/// </summary>
		public string? Description { get; init; }

		/// <summary>
		/// The id of the unit of the product.
		/// </summary>
		public Guid? UnitId { get; init; }
	}
}
