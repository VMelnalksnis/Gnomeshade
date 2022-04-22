// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel.DataAnnotations;

using JetBrains.Annotations;

namespace Gnomeshade.Interfaces.WebApi.Models.Products;

/// <summary>Information needed to create a category.</summary>
[PublicAPI]
public sealed class CategoryCreation
{
	/// <inheritdoc cref="Category.Name"/>
	[Required]
	public string Name { get; init; } = null!;

	/// <inheritdoc cref="Category.Description"/>
	public string? Description { get; init; }

	/// <inheritdoc cref="Category.CategoryId"/>
	public Guid? CategoryId { get; init; }
}
