// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Microsoft.Extensions.Options;

namespace Gnomeshade.Avalonia.Core.DesignTime;

internal sealed class DesignTimeOptionsMonitor<T> : IOptionsMonitor<T>
	where T : new()
{
	public DesignTimeOptionsMonitor()
		: this(new())
	{
	}

	public DesignTimeOptionsMonitor(T currentValue)
	{
		CurrentValue = currentValue;
	}

	/// <inheritdoc />
	public T CurrentValue { get; }

	/// <inheritdoc />
	public T Get(string name) => throw new NotImplementedException();

	/// <inheritdoc />
	public IDisposable OnChange(Action<T, string> listener) => throw new NotImplementedException();
}
