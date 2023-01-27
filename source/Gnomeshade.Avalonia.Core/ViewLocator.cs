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

namespace Gnomeshade.Avalonia.Core;

/// <summary>Data template for locating the <see cref="IView{TView,TViewModel}"/> from the calling assembly.</summary>
/// <typeparam name="TAssembly">A type from the assembly in which to search for views.</typeparam>
[RequiresUnreferencedCode("Uses System.Assembly.GetCallingAssembly().GetTypes()")]
public sealed class ViewLocator<TAssembly> : IDataTemplate
{
	// Instantiated only once on application startup
	// ReSharper disable once StaticMemberInGenericType
	private static readonly Dictionary<Type, Type> _viewDictionary = new();
	private static readonly Assembly _assembly = typeof(TAssembly).Assembly;

	/// <inheritdoc />
	public IControl Build(object data)
	{
		var dataType = data.GetType();
		if (_viewDictionary.TryGetValue(dataType, out var viewType))
		{
			return CreateControl(viewType);
		}

		viewType = _assembly.GetTypes().Single(type =>
			type.GetInterfaces().Any(i =>
				i.IsGenericType &&
				i.GetGenericTypeDefinition() == typeof(IView<,>) &&
				i.GetGenericArguments().Length >= 2 &&
				i.GetGenericArguments()[1] == dataType) &&
			type.IsAssignableTo(typeof(IControl)));

		_viewDictionary.Add(dataType, viewType);
		return CreateControl(viewType);
	}

	/// <inheritdoc />
	public bool Match(object data) => data is ViewModelBase;

	private static IControl CreateControl(Type controlType) => (IControl)Activator.CreateInstance(controlType)!;
}
