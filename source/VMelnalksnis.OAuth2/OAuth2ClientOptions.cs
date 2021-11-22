// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.ComponentModel.DataAnnotations;

using JetBrains.Annotations;

namespace VMelnalksnis.OAuth2;

[UsedImplicitly(ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.Members)]
public class OAuth2ClientOptions
{
	[Required]
	public Uri BaseAddress { get; set; } = null!;

	[Required]
	public string ClientId { get; set; } = null!;

	public string? ClientSecret { get; set; }

	public TimeSpan PollingBackoff { get; set; } = TimeSpan.FromSeconds(1);

	[Required]
	public Uri Device { get; set; } = null!;

	[Required]
	public Uri Token { get; set; } = null!;

	[Required]
	public Uri Revoke { get; set; } = null!;
}
