// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Text.Json.Serialization;

using static VMelnalksnis.OAuth2.FieldNames;

namespace VMelnalksnis.OAuth2.Responses;

public sealed record DeviceAuthorizationResponse(
	[property: JsonPropertyName(_deviceCode)] string DeviceCode,
	[property: JsonPropertyName(_userCode)] string UserCode,
	[property: JsonPropertyName("verification_uri")] Uri VerificationUri,
	[property: JsonPropertyName("verification_uri_complete")] Uri? VerificationUriComplete,
	[property: JsonPropertyName("expires_in")] int ExpiresIn,
	[property: JsonPropertyName("interval")] int Interval);
