// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Bogus.DataSets;

namespace Gnomeshade.TestingHelpers.Data.Fakers
{
	internal static class DateExtensions
	{
		internal static DateTimeOffset RecentUtc(this Date date) => new(date.Recent(1, DateTime.UtcNow), TimeSpan.Zero);
	}
}
