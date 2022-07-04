// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using JetBrains.Annotations;

namespace Gnomeshade.Interfaces.Avalonia.Core.Configuration;

/// <summary>All user configurable settings.</summary>
[UsedImplicitly(ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.Members)]
public sealed class UserConfiguration
{
	/// <summary>Gets or sets oIDC settings.</summary>
	public OidcOptions? Oidc { get; set; }

	/// <summary>Gets or sets gnomeshade settings.</summary>
	public GnomeshadeOptions? Gnomeshade { get; set; }
}
