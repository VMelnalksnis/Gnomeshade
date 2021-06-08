// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using FluentAssertions.Equivalency;

using Tracking.Finance.Data.Models.Abstractions;

namespace Tracking.Finance.Data.Tests.Integration.Repositories
{
	public static class FluentAssertionsConfiguration
	{
		public static EquivalencyAssertionOptions<TEntity> Options<TEntity>(EquivalencyAssertionOptions<TEntity> options)
			where TEntity : IEntity
		{
			return options
				.ComparingByMembers<TEntity>()
				.Excluding(entity => entity.CreatedAt);
		}

		public static EquivalencyAssertionOptions<TModifiable> ModifiableOptions<TModifiable>(EquivalencyAssertionOptions<TModifiable> options)
			where TModifiable : IEntity, IModifiableEntity
		{
			return Options(options).Excluding(modifiable => modifiable.ModifiedAt);
		}

		public static EquivalencyAssertionOptions<TModifiable> ModifiableWithoutIdOptions<TModifiable>(EquivalencyAssertionOptions<TModifiable> options)
			where TModifiable : IEntity, IModifiableEntity
		{
			return ModifiableOptions(options).Excluding(modifiable => modifiable.Id);
		}
	}
}
