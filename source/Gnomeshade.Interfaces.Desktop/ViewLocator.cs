// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Avalonia.Controls;
using Avalonia.Controls.Templates;

using Gnomeshade.Interfaces.Desktop.ViewModels;

namespace Gnomeshade.Interfaces.Desktop;

public sealed class ViewLocator : IDataTemplate
{
	public bool SupportsRecycling => false;

	/// <inheritdoc />
	public IControl Build(object data)
	{
		var name = data.GetType().FullName!.Replace("ViewModel", "View");
		var type = Type.GetType(name);

		if (type != null)
		{
			return (Control)Activator.CreateInstance(type)!;
		}

		return new TextBlock { Text = $"Not Found: {name}" };
	}

	/// <inheritdoc />
	public bool Match(object data) => data is ViewModelBase;
}
