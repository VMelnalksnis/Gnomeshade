// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel;

namespace Gnomeshade.Avalonia.Core;

/// <summary>Service for indicating the activity of the application to the user.</summary>
public interface IActivityService : INotifyPropertyChanged
{
	/// <summary>Gets a value indicating whether the application is busy.</summary>
	bool IsBusy { get; }

	/// <summary>Creates a new <see cref="IDisposable"/> whose lifetime represents the activity lifetime.</summary>
	/// <returns>An <see cref="IDisposable"/> whose lifetime represents the lifetime of the activity.</returns>
	IDisposable BeginActivity();
}
