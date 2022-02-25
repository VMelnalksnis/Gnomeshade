// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Avalonia.Controls;
using Avalonia.Controls.Templates;

using Gnomeshade.Interfaces.Avalonia.Core;

namespace Gnomeshade.Interfaces.Desktop;

public sealed class ViewLocator : IDataTemplate
{
	private static readonly Dictionary<Type, Type> _viewDictionary = new();

	public bool SupportsRecycling => false;

	/// <inheritdoc />
	public IControl Build(object data)
	{
		var dataType = data.GetType();
		if (!_viewDictionary.TryGetValue(dataType, out var viewType))
		{
			var interfaceType = typeof(IView<>).MakeGenericType(dataType);
			viewType = Assembly
				.GetExecutingAssembly()
				.GetTypes()
				.Single(type => type.IsAssignableTo(interfaceType));

			_viewDictionary.Add(dataType, viewType);
		}

		return (Control)Activator.CreateInstance(viewType)!;
	}

	/// <inheritdoc />
	public bool Match(object data) => data is ViewModelBase;
}
