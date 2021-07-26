// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Xml.Serialization;

namespace Gnomeshade.Core.Imports.Fidavista
{
	[XmlRoot("TrxSet")]
	public sealed record Transaction
	{
		[XmlElement("TypeCode")]
		public string? TypeCode { get; init; }

		[XmlElement("TypeName")]
		public string? TypeName { get; init; }

		[XmlElement("RegDate")]
		public DateTime? RegistrationDate { get; init; }

		[XmlElement("BookDate")]
		public DateTime BookDate { get; init; }

		[XmlElement("ValueDate")]
		public DateTime? ValueDate { get; init; }

		[XmlElement("ExtId")]
		public string? ExternalId { get; init; }

		[XmlElement("BenExtId")]
		public string? MassPaymentExternalId { get; init; }

		[XmlElement("EndToEndId")]
		public string? EndToEndId { get; init; }

		[XmlElement("BankRef")]
		public string BankReference { get; init; } = null!;

		[XmlElement("DocNo")]
		public string? DocumentNumber { get; init; }

		[XmlElement("CorD")]
		public string CreditOrDebit { get; init; } = null!;

		[XmlElement("AccAmt")]
		public decimal AccountAmount { get; init; }

		[XmlElement("FeeAmt")]
		public decimal? FeeAmount { get; init; }

		[XmlElement("PmtInfo")]
		public string PaymentInfo { get; init; } = null!;

		[XmlElement("StrdRef")]
		public string? ReceiverId { get; init; }

		[XmlElement("CPartySet")]
		public Counterparty[]? Counterparties { get; init; }
	}
}
