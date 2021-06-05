using System.Threading.Tasks;

using Tracking.Finance.Interfaces.WebApi.v1_0.Authentication;

namespace Tracking.Finance.Interfaces.WebApi.Client
{
	public interface IFinanceClient
	{
		Task<LoginResponse> Login(LoginModel login);
	}
}
