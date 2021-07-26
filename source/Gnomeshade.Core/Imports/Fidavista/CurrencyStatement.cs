// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Xml.Serialization;

namespace Gnomeshade.Core.Imports.Fidavista
{
	[XmlRoot("CcyStmt")]
	public sealed record CurrencyStatement
	{
		[XmlElement("Ccy")]
		public string Currency { get; init; } = null!;

		[XmlElement("OpenBal")]
		public decimal OpeningBalance { get; init; }

		[XmlElement("CloseBal")]
		public decimal? ClosingBalance { get; init; }

		[XmlElement("TrxSet")]
		public Transaction[]? Transactions { get; init; }
	}
}
