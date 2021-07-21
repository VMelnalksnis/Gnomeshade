// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

namespace Tracking.Finance.Data.Repositories.Extensions
{
	/// <summary>
	/// Represents a one to one relationship between <typeparamref name="T1"/> and <typeparamref name="T2"/>.
	/// </summary>
	/// <typeparam name="T1">The first type in the relationship.</typeparam>
	/// <typeparam name="T2">The second type in the relationship.</typeparam>
	public readonly struct OneToOne<T1, T2>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="OneToOne{T1, T2}"/> struct.
		/// </summary>
		/// <param name="first">The first value of the relationship.</param>
		/// <param name="second">The second value of the relationship.</param>
		public OneToOne(T1 first, T2 second)
		{
			First = first;
			Second = second;
		}

		/// <summary>
		/// Gets the first value of the relationship.
		/// </summary>
		public T1 First { get; }

		/// <summary>
		/// Gets the second value of relationship.
		/// </summary>
		public T2 Second { get; }
	}
}
