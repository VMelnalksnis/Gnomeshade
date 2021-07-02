// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Tracking.Finance.Interfaces.WebApi.V1_0.Authentication;

namespace Tracking.Finance.Interfaces.WebApi.Client.Login
{
	/// <summary>
	/// Indicates a successful login.
	/// </summary>
	/// <param name="Response">The information return after successful login.</param>
	public sealed record SuccessfulLogin(LoginResponse Response) : LoginResult;
}
