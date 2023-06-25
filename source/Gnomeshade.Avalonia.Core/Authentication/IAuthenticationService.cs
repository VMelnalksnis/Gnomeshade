// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Threading.Tasks;

using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Client.Results;
using Gnomeshade.WebApi.Models.Authentication;

namespace Gnomeshade.Avalonia.Core.Authentication;

/// <summary>Handles authentication from different providers.</summary>
public interface IAuthenticationService
{
	/// <summary>Logs in using an external identity provider.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task<ExternalLoginResult> SocialLogin();

	/// <summary>Logs in an application user.</summary>
	/// <param name="login">Parameters needed for logging in.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task<LoginResult> Login(Login login);

	/// <summary>Logs out the current user.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task Logout();
}
