// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

namespace Tracking.Finance.Data.Repositories.Extensions
{
	/// <summary>
	/// Represents a relationship between a single <typeparamref name="TChild"/> and <typeparamref name="TParent"/> in a one to many relationship.
	/// </summary>
	/// <typeparam name="TParent">The type of the parent (single) object.</typeparam>
	/// <typeparam name="TChild">The type of the children (many) objects.</typeparam>
	public readonly struct OneToManyEntry<TParent, TChild>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="OneToManyEntry{TParent, TChild}"/> struct from parent and child objects.
		/// </summary>
		/// <param name="parent">The parent object.</param>
		/// <param name="child">The child object.</param>
		public OneToManyEntry(TParent parent, TChild child)
		{
			Parent = parent;
			Child = child;
		}

		/// <summary>
		/// Gets the parent (single) object.
		/// </summary>
		public TParent Parent { get; }

		/// <summary>
		/// Gets one of the child (many) objects.
		/// </summary>
		public TChild Child { get; }
	}
}
