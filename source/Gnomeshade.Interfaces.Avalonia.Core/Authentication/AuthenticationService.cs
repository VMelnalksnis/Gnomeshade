// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Threading.Tasks;

using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models.Authentication;

namespace Gnomeshade.Interfaces.Avalonia.Core.Authentication;

/// <inheritdoc />
public sealed class AuthenticationService : IAuthenticationService
{
	private readonly IGnomeshadeClient _gnomeshadeClient;

	/// <summary>Initializes a new instance of the <see cref="AuthenticationService"/> class.</summary>
	/// <param name="gnomeshadeClient">API client for authenticating within the app.</param>
	public AuthenticationService(IGnomeshadeClient gnomeshadeClient)
	{
		_gnomeshadeClient = gnomeshadeClient;
	}

	/// <inheritdoc />
	public Task SocialLogin() => _gnomeshadeClient.SocialRegister();

	/// <inheritdoc />
	public Task<LoginResult> Login(Login login) => _gnomeshadeClient.LogInAsync(login);

	/// <inheritdoc />
	public Task Logout() => _gnomeshadeClient.LogOutAsync();
}
