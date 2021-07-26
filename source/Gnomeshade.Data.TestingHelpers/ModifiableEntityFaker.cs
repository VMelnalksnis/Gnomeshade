// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq.Expressions;

using Bogus;

using Gnomeshade.Data.Models.Abstractions;

namespace Gnomeshade.Data.TestingHelpers
{
	public abstract class ModifiableEntityFaker<TEntity> : Faker<TEntity>
		where TEntity : class, IModifiableEntity, IOwnableEntity
	{
		protected ModifiableEntityFaker(Guid userId)
		{
			RuleFor(entity => entity.OwnerId, userId);
			RuleFor(entity => entity.CreatedByUserId, userId);
			RuleFor(entity => entity.ModifiedByUserId, userId);
		}

		/// <inheritdoc />
		public sealed override Faker<TEntity> RuleFor<TProperty>(
			Expression<Func<TEntity, TProperty>> property,
			TProperty value)
		{
			return base.RuleFor(property, value);
		}
	}
}
