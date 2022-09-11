// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading.Tasks;

using VMelnalksnis.PaperlessDotNet.Documents;

namespace Gnomeshade.WebApi.V1.Importing.Paperless;

/// <summary>Service for integrating with Paperless.</summary>
/// <seealso href="https://github.com/paperless-ngx/paperless-ngx"/>
public interface IPaperlessService
{
	/// <summary>Checks whether the specified <paramref name="uri"/> represents a paperless <see cref="Document"/>.</summary>
	/// <param name="uri">The uri to check.</param>
	/// <returns><c>true</c> if <paramref name="uri"/> links to a paperless <see cref="Document"/>; otherwise <c>false</c>.</returns>
	public bool IsPaperlessDocumentUri(string uri);

	/// <summary>Gets the paperless <see cref="Document"/> at the specified <paramref name="uri"/>.</summary>
	/// <param name="uri">The uri of the document.</param>
	/// <returns>The document if it exists; otherwise <c>null</c>.</returns>
	/// <seealso cref="IDocumentClient.Get"/>
	public Task<Document?> GetPaperlessDocument(string uri);

	/// <summary>Adds purchases parsed from <paramref name="document"/> to <paramref name="transactionId"/>.</summary>
	/// <param name="ownerId">The id of the owner of the <paramref name="transactionId"/> and purchases to add.</param>
	/// <param name="transactionId">The id of the transaction to which to add the purchases.</param>
	/// <param name="document">The document from which to parse the purchases.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task AddPurchasesToTransaction(Guid ownerId, Guid transactionId, Document document);
}
