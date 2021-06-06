// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU General Public License 3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Threading.Tasks;

using Tracking.Finance.Interfaces.WebApi.V1_0.Authentication;

namespace Tracking.Finance.Interfaces.WebApi.Client
{
	public interface IFinanceClient
	{
		Task<UserModel> Info();

		Task<LoginResponse> Login(LoginModel login);
	}
}
