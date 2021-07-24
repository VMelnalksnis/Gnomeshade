// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Xml.Serialization;

namespace Tracking.Finance.Imports.Fidavista
{
	[XmlRoot("AccountSet")]
	public sealed record Account
	{
		[XmlElement("IBAN")]
		public string? Iban { get; init; }

		[XmlElement("AccNo")]
		public string AccountNumber { get; init; } = null!;

		[XmlElement("SubAccNo")]
		public string? SubAccountNumber { get; init; }

		[XmlElement("AccType")]
		public string? AccountType { get; init; }

		[XmlElement("CcyStmt")]
		public CurrencyStatement[]? Statements { get; init; }
	}
}
