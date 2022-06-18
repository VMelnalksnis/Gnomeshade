﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Gnomeshade.Interfaces.WebApi.Models.Importing;

namespace Gnomeshade.Interfaces.WebApi.Client;

/// <summary>Provides typed interface for using the import API.</summary>
public interface IImportClient
{
	/// <summary>Imports an ISO 20022 Bank-To-Customer Account Report v02.</summary>
	/// <param name="content">A stream of a CAMT.052.001.02 message.</param>
	/// <param name="name">The name of the file containing the <paramref name="content"/>.</param>
	/// <returns>A summary of the imported data.</returns>
	Task<AccountReportResult> Import(Stream content, string name);

	/// <summary>Gets all institutions that operate within the specified country.</summary>
	/// <param name="countryCode">An ISO 3166 two-character country code.</param>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>All institutions from the specified country.</returns>
	Task<List<string>> GetInstitutionsAsync(string countryCode, CancellationToken cancellationToken = default);

	/// <summary>Imports all transactions from the specified institution.</summary>
	/// <param name="id">The institution id.</param>
	/// <returns>A summary of the imported data.</returns>
	Task<List<AccountReportResult>> ImportAsync(string id);
}
