// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;

namespace Tracking.Finance.Data.Repositories.Extensions
{
	/// <summary>
	/// Represents a one to many relationship between one <typeparamref name="TParent"/> and many <typeparamref name="TChild"/>.
	/// </summary>
	/// <typeparam name="TParent">The type of the parent (single) object.</typeparam>
	/// <typeparam name="TChild">The type of the children (many) objects.</typeparam>
	public readonly struct OneToMany<TParent, TChild>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="OneToMany{TParent, TChild}"/> struct from parent and child objects.
		/// </summary>
		/// <param name="parent">The parent object.</param>
		/// <param name="children">The collection of child objects.</param>
		public OneToMany(TParent parent, List<TChild> children)
		{
			Parent = parent;
			Children = children;
		}

		/// <summary>
		/// Gets the parent (single) object.
		/// </summary>
		public TParent Parent { get; }

		/// <summary>
		/// Gets the child (many) objects.
		/// </summary>
		public List<TChild> Children { get; }
	}
}
