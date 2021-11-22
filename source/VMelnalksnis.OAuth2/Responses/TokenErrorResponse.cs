// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Text.Json.Serialization;

namespace VMelnalksnis.OAuth2.Responses;

public sealed record TokenErrorResponse(
	[property: JsonPropertyName("error")] string Error,
	[property: JsonPropertyName("error_description")] string ErrorDescription);
