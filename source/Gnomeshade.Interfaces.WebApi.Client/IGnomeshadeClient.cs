// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Gnomeshade.Interfaces.WebApi.Models;
using Gnomeshade.Interfaces.WebApi.Models.Authentication;

namespace Gnomeshade.Interfaces.WebApi.Client;

/// <summary>Provides typed interface for using the API provided by the Interfaces.WebApi project.</summary>
public interface IGnomeshadeClient : IAccountClient, IProductClient, ITransactionClient, IImportClient, IOwnerClient
{
	/// <summary>Log in using the specified credentials.</summary>
	/// <param name="login">The credentials to use to log in.</param>
	/// <returns>Object indicating whether the login was successful or not.</returns>
	Task<LoginResult> LogInAsync(Login login);

	/// <summary>Register using an OIDC provider token.</summary>
	/// <param name="accessToken">An access token from an OIDC provider.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task SocialRegister(string accessToken);

	/// <summary>Log out.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task LogOutAsync();

	/// <summary>Gets all links.</summary>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>All links.</returns>
	Task<List<Link>> GetLinksAsync(CancellationToken cancellationToken = default);

	/// <summary>Gets the specified link.</summary>
	/// <param name="id">The id of the link to get.</param>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>The link with the specified id.</returns>
	Task<Link> GetLinkAsync(Guid id, CancellationToken cancellationToken = default);

	/// <summary>Creates a new link or replaces an existing  one, if one exists with the specified id.</summary>
	/// <param name="id">The id of the link.</param>
	/// <param name="link">The link to create or replace.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task PutLinkAsync(Guid id, LinkCreation link);

	/// <summary>Deletes the specified link.</summary>
	/// <param name="id">The id of the link to delete.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task DeleteLinkAsync(Guid id);
}
