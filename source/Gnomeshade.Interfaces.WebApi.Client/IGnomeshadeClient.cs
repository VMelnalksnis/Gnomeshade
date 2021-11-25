// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Threading.Tasks;

using Gnomeshade.Interfaces.WebApi.Models.Authentication;

namespace Gnomeshade.Interfaces.WebApi.Client
{
	/// <summary>
	/// Provides typed interface for using the API provided by the Interfaces.WebApi project.
	/// </summary>
	public interface IGnomeshadeClient : IAccountClient, IProductClient, ITransactionClient, IImportClient
	{
		/// <summary>
		/// Log in using the specified credentials.
		/// </summary>
		/// <param name="login">The credentials to use to log in.</param>
		/// <returns>Object indicating whether the login was successful or not.</returns>
		Task<LoginResult> LogInAsync(Login login);

		/// <summary>
		/// Register using an OIDC provider token.
		/// </summary>
		/// <param name="accessToken">An access token from an OIDC provider.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task SocialRegister(string accessToken);

		/// <summary>
		/// Log out.
		/// </summary>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task LogOutAsync();
	}
}
