// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.Data.Models;

namespace Gnomeshade.Data.TestingHelpers
{
	/// <summary>
	/// Generates fake <see cref="Product"/> objects.
	/// </summary>
	public sealed class ProductFaker : ModifiableEntityFaker<Product>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ProductFaker"/> class with the specified relationships.
		/// </summary>
		/// <param name="user">The user which created this product.</param>
		public ProductFaker(User user)
			: this(user.Id)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ProductFaker"/> class with the specified relationship ids.
		/// </summary>
		/// <param name="userId">The id of the <see cref="User"/> which created this product.</param>
		public ProductFaker(Guid userId)
			: base(userId)
		{
			RuleFor(product => product.Name, faker => faker.Commerce.ProductName());
			RuleFor(product => product.NormalizedName, (_, product) => product.Name.ToUpperInvariant());
			RuleFor(product => product.Description, faker => faker.Commerce.ProductDescription());
		}
	}
}
