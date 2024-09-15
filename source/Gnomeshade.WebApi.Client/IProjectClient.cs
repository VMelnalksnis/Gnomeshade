// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Gnomeshade.WebApi.Models.Projects;
using Gnomeshade.WebApi.Models.Transactions;

namespace Gnomeshade.WebApi.Client;

/// <summary>Provides typed interface for using the project API.</summary>
public interface IProjectClient
{
	/// <summary>Gets all projects.</summary>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>A collection with all projects.</returns>
	Task<List<Project>> GetProjectsAsync(CancellationToken cancellationToken = default);

	/// <summary>Gets the project with the specified id.</summary>
	/// <param name="id">The id by which to search for the project.</param>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>The project with the specified id.</returns>
	Task<Project> GetProjectAsync(Guid id, CancellationToken cancellationToken = default);

	/// <summary>Creates a new project.</summary>
	/// <param name="project">The project to create.</param>
	/// <returns>The id of the created project.</returns>
	Task<Guid> CreateProjectAsync(ProjectCreation project);

	/// <summary>Creates a new project, or replaces and existing one if one exists with the specified id.</summary>
	/// <param name="id">The id of the project.</param>
	/// <param name="project">The project to create or update.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task PutProjectAsync(Guid id, ProjectCreation project);

	/// <summary>Deletes the specified project.</summary>
	/// <param name="id">The id of the project to delete.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task DeleteProjectAsync(Guid id);

	/// <summary>Gets all purchases that are a part of the specified project.</summary>
	/// <param name="id">The id of the project for which to get all the purchases.</param>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>All purchases that are a part of the specified project.</returns>
	Task<List<Purchase>> GetProjectPurchasesAsync(Guid id, CancellationToken cancellationToken = default);

	/// <summary>Adds the specified purchase to the specified project.</summary>
	/// <param name="id">The id of the project to which to add the purchase.</param>
	/// <param name="purchaseId">The id of the purchase which to add to the project.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task AddPurchaseToProjectAsync(Guid id, Guid purchaseId);

	/// <summary>Removes the specified purchase to the specified project.</summary>
	/// <param name="id">The id of the project from which to remove the purchase.</param>
	/// <param name="purchaseId">The id of the purchase which to remove from the project.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task RemovePurchaseFromProjectAsync(Guid id, Guid purchaseId);
}
