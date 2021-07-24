// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Xml.Serialization;

namespace Tracking.Finance.Imports.Fidavista
{
	[XmlRoot("Statement")]
	public sealed record Statement
	{
		[XmlElement("Period")]
		public Period Period { get; init; } = null!;

		[XmlElement("AccountSet")]
		public Account[]? Accounts { get; init; }
	}
}
