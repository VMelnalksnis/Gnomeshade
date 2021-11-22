// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Text.Json.Serialization;

using static VMelnalksnis.OAuth2.FieldNames;

namespace VMelnalksnis.OAuth2.Responses;

public sealed record TokenResponse(
	[property: JsonPropertyName("access_token")] string AccessToken,
	[property: JsonPropertyName("id_token")] string IdToken,
	[property: JsonPropertyName(_refreshToken)] string RefreshToken,
	[property: JsonPropertyName("token_type")] string TokenType,
	[property: JsonPropertyName("expires_in")] int ExpiresIn,
	[property: JsonPropertyName(_scope)] string Scope);
