// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

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

	private static readonly Type[] _viewTypes = typeof(TAssembly)
		.Assembly
		.GetTypes()
		.Where(type =>
			type.GetInterfaces().Any(interfaceType =>
				interfaceType.IsGenericType &&
				interfaceType.GetGenericTypeDefinition() == typeof(IView<,>)) &&
			type.IsAssignableTo(typeof(IControl))).ToArray();

	/// <inheritdoc />
	public IControl Build(object data)
	{
		var dataType = data.GetType();
		if (_viewDictionary.TryGetValue(dataType, out var viewType))
		{
			return CreateControl(viewType);
		}

		viewType = _viewTypes.Single(type =>
			type.GetInterfaces().Any(interfaceType =>
				interfaceType.IsGenericType &&
				interfaceType.GetGenericTypeDefinition() == typeof(IView<,>) &&
				interfaceType.GetGenericArguments()[1] == dataType));

		_viewDictionary.Add(dataType, viewType);
		return CreateControl(viewType);
	}

	/// <inheritdoc />
	public bool Match(object data) => data is ViewModelBase;

	private static IControl CreateControl(
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
		Type controlType) =>
		(IControl)Activator.CreateInstance(controlType)!;
}
