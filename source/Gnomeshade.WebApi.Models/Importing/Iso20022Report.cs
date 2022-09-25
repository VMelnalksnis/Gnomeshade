// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.ComponentModel.DataAnnotations;

using JetBrains.Annotations;

using Microsoft.AspNetCore.Http;

namespace Gnomeshade.WebApi.Models.Importing;

/// <summary>An ISO20022 report.</summary>
[PublicAPI]
public sealed record Iso20022Report
{
	/// <summary>The ISO20022 report content.</summary>
	[Required]
	public IFormFile Report { get; set; } = null!;

	/// <summary>The timezone which will be assumed for all unspecified dates in the report.</summary>
	[Required]
	public string TimeZone { get; set; } = null!;
}
