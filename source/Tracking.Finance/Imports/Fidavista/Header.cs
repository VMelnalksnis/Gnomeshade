// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Xml.Serialization;

namespace Tracking.Finance.Imports.Fidavista
{
	public sealed record Header
	{
		[XmlElement("Timestamp")]
		public string? Timestamp { get; init; }

		[XmlElement("From")]
		public string? From { get; init; }
	}
}
