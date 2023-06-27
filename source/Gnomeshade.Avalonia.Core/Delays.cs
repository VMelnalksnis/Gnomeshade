// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

namespace Gnomeshade.Avalonia.Core;

internal static class Delays
{
	internal static readonly TimeSpan ActivityDelay = TimeSpan.FromMilliseconds(_activityMillis);
	internal static readonly TimeSpan UserInputDelay = TimeSpan.FromMilliseconds(_userInputMillis);

	private const double _activityMillis = 100;
	private const double _userInputMillis = 350;
}
