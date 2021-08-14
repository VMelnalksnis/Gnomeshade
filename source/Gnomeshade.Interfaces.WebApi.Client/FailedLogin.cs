// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Net;

namespace Gnomeshade.Interfaces.WebApi.Client
{
	/// <summary>
	/// Indicates a failed login.
	/// </summary>
	/// <param name="StatusCode">The response status code.</param>
	/// <param name="Message">Message describing the failure reason.</param>
	public sealed record FailedLogin(HttpStatusCode? StatusCode, string Message) : LoginResult;
}
