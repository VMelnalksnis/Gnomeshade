// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel.DataAnnotations;

using JetBrains.Annotations;

namespace Gnomeshade.Interfaces.WebApi.Configuration
{
	[UsedImplicitly(ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.Members)]
	public sealed class KeycloakOptions
	{
		public const string SectionName = "Keycloak";

		[Required]
		public Uri ServerRealm { get; set; } = null!;

		[Required]
		public Uri Metadata { get; set; } = null!;

		[Required]
		public string ClientId { get; set; } = null!;

		[Required]
		public string ClientSecret { get; set; } = null!;

		[Required]
		public string CookieName { get; set; } = "keycloak.cookie";

		[Required]
		public string RemoteSignOutPath { get; set; } = "/protocol/openid-connect/logout";

		[Required]
		public string SignedOutRedirectUri { get; set; } = "/swagger";
	}
}
