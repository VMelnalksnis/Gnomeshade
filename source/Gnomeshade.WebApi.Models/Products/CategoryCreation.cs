// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel.DataAnnotations;

using JetBrains.Annotations;

namespace Gnomeshade.WebApi.Models.Products;

/// <summary>Information needed to create a category.</summary>
[PublicAPI]
public sealed record CategoryCreation : Creation
{
	/// <inheritdoc cref="Category.Name"/>
	[Required]
	public string Name { get; set; } = null!;

	/// <inheritdoc cref="Category.Description"/>
	public string? Description { get; set; }

	/// <inheritdoc cref="Category.CategoryId"/>
	public Guid? CategoryId { get; set; }

	/// <summary>Whether to create a linked product for using this category in purchases.</summary>
	/// <seealso cref="Category.LinkedProductId"/>
	public bool LinkProduct { get; set; }
}
