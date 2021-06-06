// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU General Public License 3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Tracking.Finance.Interfaces.WindowsDesktop.ViewModels
{
	public interface IViewModel
	{
		/// <summary>
		/// Gets a collection of all types that implement <see cref="IViewModel"/>
		/// from the <see cref="Assembly"/> which contains <typeparamref name="T"/>.
		/// </summary>
		///
		/// <typeparam name="T">The type used to specify the assembly to search in.</typeparam>
		/// <returns>All classes from the specified assembly implementing <see cref="IViewModel"/>.</returns>
		public static IEnumerable<Type> GetViewModels<T>()
		{
			return typeof(T).Assembly
				.GetTypes()
				.Where(type => type.IsClass && type.IsAssignableTo(typeof(IViewModel)));
		}
	}
}
