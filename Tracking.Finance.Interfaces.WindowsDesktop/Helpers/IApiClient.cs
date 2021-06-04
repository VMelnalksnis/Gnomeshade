using System.Threading.Tasks;

using Tracking.Finance.Interfaces.WindowsDesktop.Models;

namespace Tracking.Finance.Interfaces.WindowsDesktop.Helpers
{
	public interface IApiClient
	{
		Task<AuthenticatedUser?> Authenticate(string username, string password);
	}
}
