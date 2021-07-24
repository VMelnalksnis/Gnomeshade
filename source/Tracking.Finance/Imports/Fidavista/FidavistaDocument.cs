// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Xml.Serialization;

namespace Tracking.Finance.Imports.Fidavista
{
	[XmlRoot("FIDAVISTA", Namespace = @"http://ivis.eps.gov.lv/XMLSchemas/100017/fidavista/v1-2")]
	public sealed record FidavistaDocument
	{
		[XmlElement("Header")]
		public Header? Header { get; init; }

		[XmlElement("Statement")]
		public Statement[]? Statements { get; init; }
	}
}
