// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;

using Gnomeshade.WebApi.Models.Importing;

namespace Gnomeshade.WebApi.Client;

/// <summary>Transactions have been successfully imported.</summary>
public sealed class SuccessfulImport : ImportResult
{
	/// <summary>Initializes a new instance of the <see cref="SuccessfulImport"/> class.</summary>
	/// <param name="results">The summaries of reported data for each account.</param>
	public SuccessfulImport(List<AccountReportResult> results)
	{
		Results = results;
	}

	/// <summary>Gets the summaries of reported data for each account.</summary>
	public List<AccountReportResult> Results { get; }
}
