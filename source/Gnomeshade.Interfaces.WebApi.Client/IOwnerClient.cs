// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Gnomeshade.Interfaces.WebApi.Models.Owners;

namespace Gnomeshade.Interfaces.WebApi.Client;

/// <summary>Provides typed interface for managing access rights to resources.</summary>
public interface IOwnerClient
{
	/// <summary>Gets all accesses.</summary>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>A collection of all accesses.</returns>
	Task<List<Access>> GetAccessesAsync(CancellationToken cancellationToken = default);

	/// <summary>Gets all owners.</summary>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>A collection of all owners.</returns>
	Task<List<Owner>> GetOwnersAsync(CancellationToken cancellationToken = default);

	/// <summary>Gets all ownerships.</summary>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>A collection of all ownerships.</returns>
	Task<List<Ownership>> GetOwnershipsAsync(CancellationToken cancellationToken = default);

	/// <summary>Creates a new ownership or replaces an existing one, if one exists with the specified id.</summary>
	/// <param name="id">The id of the ownership.</param>
	/// <param name="ownership">The ownership to create or replace.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task PutOwnershipAsync(Guid id, OwnershipCreation ownership);

	/// <summary>Deletes the specified ownership.</summary>
	/// <param name="id">The id of the ownership to delete.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task DeleteOwnershipAsync(Guid id);
}
