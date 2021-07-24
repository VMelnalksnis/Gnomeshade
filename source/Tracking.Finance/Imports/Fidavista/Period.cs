// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Xml.Serialization;

namespace Tracking.Finance.Imports.Fidavista
{
	public sealed record Period
	{
		[XmlElement("StartDate")]
		public DateTime StartDate { get; init; }

		[XmlElement("EndDate")]
		public DateTime EndDate { get; init; }

		[XmlElement("PrepDate")]
		public DateTime PreparationDate { get; init; }
	}
}
