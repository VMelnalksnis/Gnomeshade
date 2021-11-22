// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Web;

namespace VMelnalksnis.OAuth2.Responses;

public static class DeviceAuthorizationResponseExtensions
{
	/// <summary>
	/// Get <see cref="T:System.Diagnostics.ProcessStartInfo" /> for opening the <see cref="P:VMelnalksnis.OAuth2.DeviceAuthorizationResponse.VerificationUri" />,
	/// so that the user can approve/deny the request.
	/// </summary>
	/// <param name="response"></param>
	/// <returns><see cref="T:System.Diagnostics.ProcessStartInfo" /> for opening the verification uri in the default browser.</returns>
	/// <exception cref="T:System.PlatformNotSupportedException">Current platform is not <see cref="P:System.Runtime.InteropServices.OSPlatform.Windows" />.</exception>
	public static ProcessStartInfo GetProcessStartInfoForUserApproval(this DeviceAuthorizationResponse response)
	{
		if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			throw new PlatformNotSupportedException();
		}

		if (response.VerificationUriComplete is not null)
		{
			return CreateProcessStartInfo(response.VerificationUriComplete);
		}

		var uriBuilder = new UriBuilder(response.VerificationUri.AbsoluteUri);
		var queryString = HttpUtility.ParseQueryString(uriBuilder.Query);
		queryString.Add(FieldNames._userCode, response.UserCode);
		uriBuilder.Query = queryString.ToString();
		return CreateProcessStartInfo(uriBuilder.Uri);
	}

	private static ProcessStartInfo CreateProcessStartInfo(Uri uri) => new(uri.AbsoluteUri)
	{
		CreateNoWindow = true,
		UseShellExecute = true,
	};
}
