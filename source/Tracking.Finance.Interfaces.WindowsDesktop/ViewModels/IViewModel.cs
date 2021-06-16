// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;

using JetBrains.Annotations;

namespace Tracking.Finance.Interfaces.WindowsDesktop.ViewModels
{
	/// <summary>
	/// Used for identifying view models for dependency injection.
	/// </summary>
	[UsedImplicitly(
		ImplicitUseKindFlags.Access,
		ImplicitUseTargetFlags.Members | ImplicitUseTargetFlags.WithInheritors)]
	public interface IViewModel
	{
		/// <summary>
		/// Gets a collection of all types that implement <see cref="IViewModel"/>
		/// from the <see cref="Assembly"/> which contains <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">The type used to specify the assembly to search in.</typeparam>
		/// <returns>All classes from the specified assembly implementing <see cref="IViewModel"/>.</returns>
		public static IEnumerable<Type> GetViewModels<T>()
		{
			foreach (var type in typeof(T).Assembly.GetTypes())
			{
				if (type.IsClass && type.IsAssignableTo(typeof(IViewModel)))
				{
					yield return type;
				}
			}
		}
	}
}
