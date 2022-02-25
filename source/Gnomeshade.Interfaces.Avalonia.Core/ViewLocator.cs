// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace Gnomeshade.Interfaces.Avalonia.Core;

/// <summary>Data template for locating the <see cref="IView{TViewModel}"/> from the calling assembly.</summary>
/// <typeparam name="TAssembly">A type from the assembly in which to search for views.</typeparam>
public sealed class ViewLocator<TAssembly> : IDataTemplate
{
	private static readonly Assembly _assembly = typeof(TAssembly).Assembly;
	private static readonly Dictionary<Type, Type> _viewDictionary = new();

	/// <inheritdoc />
	[RequiresUnreferencedCode("Uses System.Assembly.GetCallingAssembly().GetTypes()")]
	public IControl Build(object data)
	{
		var dataType = data.GetType();
		if (_viewDictionary.TryGetValue(dataType, out var viewType))
		{
			return CreateControl(viewType);
		}

		var interfaceType = typeof(IView<>).MakeGenericType(dataType);
		viewType = _assembly.GetTypes().Single(type =>
			type.IsAssignableTo(interfaceType) &&
			type.IsAssignableTo(typeof(IControl)));

		_viewDictionary.Add(dataType, viewType);
		return CreateControl(viewType);
	}

	/// <inheritdoc />
	public bool Match(object data) => data is ViewModelBase;

	private static IControl CreateControl(Type controlType) => (IControl)Activator.CreateInstance(controlType)!;
}
