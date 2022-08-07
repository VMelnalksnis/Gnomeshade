// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Text.Json;

using NodaTime;
using NodaTime.Serialization.SystemTextJson;

namespace Gnomeshade.WebApi.Client;

/// <summary><see cref="JsonSerializerOptions"/> for <see cref="IGnomeshadeClient"/>.</summary>
public sealed class GnomeshadeJsonSerializerOptions
{
	/// <summary>Initializes a new instance of the <see cref="GnomeshadeJsonSerializerOptions"/> class.</summary>
	/// <param name="dateTimeZoneProvider">Time zone provider for date and time serialization.</param>
	public GnomeshadeJsonSerializerOptions(IDateTimeZoneProvider dateTimeZoneProvider)
	{
		Options = new(JsonSerializerDefaults.Web);
		Options.ConfigureForNodaTime(dateTimeZoneProvider);
	}

	internal JsonSerializerOptions Options { get; }
}
