﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Gnomeshade.Avalonia.Core;

internal readonly struct ActivityScope : IDisposable
{
	private readonly ICollection<ActivityScope> _scopes;

	public ActivityScope(ICollection<ActivityScope> scopes, string name)
	{
		_scopes = scopes;
		Name = name;

		_scopes.Add(this);
	}

	/// <summary>Gets the name of the activity.</summary>
	public string Name { get; }

	/// <inheritdoc />
	public void Dispose() => _scopes.Remove(this);
}
