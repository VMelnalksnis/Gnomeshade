// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Microsoft.AspNetCore.Http;

namespace Gnomeshade.WebApi.Configuration;

internal static class HttpRequestExtensions
{
	internal static bool IsApiRequest(this HttpRequest request) => request.Path.StartsWithSegments(new("/api"));
}
