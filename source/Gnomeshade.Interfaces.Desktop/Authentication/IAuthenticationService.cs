using System.Threading.Tasks;

using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models.Authentication;

namespace Gnomeshade.Interfaces.Desktop.Authentication;

/// <summary>
/// Handles authentication from different providers.
/// </summary>
public interface IAuthenticationService
{
	/// <summary>
	/// Logs in using an external identity provider.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task SocialLogin();

	/// <summary>
	/// Logs in an application user.
	/// </summary>
	/// <param name="login">Parameters needed for logging in.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task<LoginResult> Login(Login login);

	/// <summary>
	/// Logs out the current user.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task Logout();
}
