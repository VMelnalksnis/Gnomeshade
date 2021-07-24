// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Xml.Serialization;

namespace Tracking.Finance.Imports.Fidavista
{
	[XmlRoot("CPartySet")]
	public sealed record Counterparty
	{
		[XmlElement("AccNo")]
		public string? AccountNumber { get; init; }

		[XmlElement("SubAccNo")]
		public string? SubAccountNumber { get; init; }

		[XmlElement("AccHolder")]
		public AccountHolder? AccountHolder { get; init; }

		[XmlElement("BankCode")]
		public string? BankCode { get; init; }

		[XmlElement("BankName")]
		public string? BankName { get; init; }

		[XmlElement("Ccy")]
		public string? Currency { get; init; }

		[XmlElement("Amt")]
		public decimal? Amount { get; init; }

		[XmlElement("CurRate")]
		public decimal? CurrencyRate { get; init; }

		[XmlElement("Giro")]
		public string? Giro { get; init; }
	}
}
