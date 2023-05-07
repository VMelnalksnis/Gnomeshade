// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.ComponentModel.DataAnnotations;

using JetBrains.Annotations;

namespace Gnomeshade.WebApi.Models.Owners;

/// <summary>Information needed to create an owner.</summary>
[PublicAPI]
public sealed record OwnerCreation : Creation
{
	/// <inheritdoc cref="Owner.Name"/>
	[Required]
	public string Name { get; set; } = null!;
}
