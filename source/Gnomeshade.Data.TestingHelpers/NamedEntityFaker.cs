// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.Data.Entities.Abstractions;

namespace Gnomeshade.Data.TestingHelpers
{
	public abstract class NamedEntityFaker<TEntity> : ModifiableEntityFaker<TEntity>
		where TEntity : class, IModifiableEntity, IOwnableEntity, INamedEntity
	{
		protected NamedEntityFaker(Guid userId)
			: base(userId)
		{
		}

		/// <summary>
		/// Generates an entity unique from <paramref name="entity"/>.
		/// </summary>
		/// <param name="entity">An entity against which to compare for uniqueness.</param>
		/// <param name="attemptCount">The number of times to try to generate a unique entity.</param>
		/// <returns>A fake fake entity unique from <paramref name="entity"/>.</returns>
		/// <exception cref="InvalidOperationException">Failed to generate a unique entity after <paramref name="attemptCount"/> attempts.</exception>
		public TEntity GenerateUnique(TEntity entity, int attemptCount = 10)
		{
			for (var i = 0; i < attemptCount; i++)
			{
				var generatedAccount = Generate();
				if (entity.NormalizedName != generatedAccount.NormalizedName)
				{
					return generatedAccount;
				}
			}

			throw new InvalidOperationException($"Failed to generate unique entity after {attemptCount} attempts");
		}
	}
}
