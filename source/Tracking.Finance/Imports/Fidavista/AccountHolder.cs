// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Xml.Serialization;

namespace Tracking.Finance.Imports.Fidavista
{
	[XmlRoot("AccHolder")]
	public sealed record AccountHolder
	{
		[XmlElement("Name")]
		public string? Name { get; init; }

		[XmlElement("LegalId")]
		public string? LegalId { get; init; }

		[XmlElement("Address")]
		public string? Address { get; init; }
	}
}
