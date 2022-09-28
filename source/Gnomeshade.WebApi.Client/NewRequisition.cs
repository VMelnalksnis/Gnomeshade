// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

namespace Gnomeshade.WebApi.Client;

/// <summary>User approval is needed in order to access the account.</summary>
public sealed class NewRequisition : ImportResult
{
	/// <summary>Initializes a new instance of the <see cref="NewRequisition"/> class.</summary>
	/// <param name="requisitionUri">The URI for the approval page.</param>
	public NewRequisition(Uri requisitionUri)
	{
		RequisitionUri = requisitionUri;
	}

	/// <summary>Gets the URI for the approval page.</summary>
	public Uri RequisitionUri { get; }
}
