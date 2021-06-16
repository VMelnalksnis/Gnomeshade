using System;

using Tracking.Finance.Web.Data.Models;

namespace Tracking.Finance.Web.Models
{
	public static class EnitityExtensions
	{
		public static TModifiable CreatedAndModifiedNow<TModifiable>(this TModifiable modifiableEntity)
			where TModifiable : IModifiableEntity
		{
			return modifiableEntity.CreatedAndModifiedAt(DateTimeOffset.Now);
		}

		public static TModifiable CreatedAndModifiedAt<TModifiable>(this TModifiable modifiableEntity, DateTimeOffset creationDate)
			where TModifiable : IModifiableEntity
		{
			modifiableEntity.CreatedAt = creationDate;
			modifiableEntity.ModifiedAt = creationDate;

			return modifiableEntity;
		}

		/// <summary>
		/// Sets <see cref="INamedEntity.Name"/> and <see cref="INamedEntity.NormalizedName"/> of the entity.
		/// </summary>
		/// <param name="name">The value to which to set the name of the entity.</param>
		/// <returns>The given enitity.</returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="name"/> is <see langword="null"/>, <see cref="string.Empty"/> or white space.
		/// </exception>
		public static TNamedEntity WithName<TNamedEntity>(this TNamedEntity namedEntity, string? name)
			where TNamedEntity : INamedEntity
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentNullException(nameof(name));
			}

			namedEntity.Name = name;
			namedEntity.NormalizedName = name.Trim().ToUpperInvariant();

			return namedEntity;
		}
	}
}
