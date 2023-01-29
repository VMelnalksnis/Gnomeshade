// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Xml.Serialization;

using VMelnalksnis.ISO20022DotNet.MessageSets.BankToCustomerCashManagement.V2.AccountReport;

namespace Gnomeshade.WebApi.V1.Importing;

/// <summary><see cref="BankToCustomerAccountReportV02"/> XML reader.</summary>
[UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Unused properties can removed from models")]
public sealed class Iso20022AccountReportReader
{
	private readonly XmlSerializer _xmlSerializer = new(typeof(Document));

	/// <summary>Reads the <see cref="BankToCustomerAccountReportV02"/> message from the XML stream.</summary>
	/// <param name="inputStream">Stream of containing the message.</param>
	/// <returns>The account report message from the stream.</returns>
	public BankToCustomerAccountReportV02 ReadReport(Stream inputStream)
	{
		var streamReader = new StreamReader(inputStream, Encoding.UTF8, true);
		var document = (Document)_xmlSerializer.Deserialize(streamReader)!;
		return document.BankToCustomerAccountReport;
	}
}
