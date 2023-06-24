// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;

using Avalonia.Controls.Notifications;

namespace Gnomeshade.Avalonia.Core.Tests;

internal sealed class StubbedActivityService : PropertyChangedBase, IActivityService
{
	public bool IsBusy => false;

	public IEnumerable<string> Activities { get; } = Array.Empty<string>();

	public IDisposable BeginActivity(string name) => new AssertionScope();

	public void ShowNotification(Notification notification)
	{
	}

	public void ShowErrorNotification(string message)
	{
	}
}
