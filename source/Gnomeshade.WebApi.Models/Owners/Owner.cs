// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using JetBrains.Annotations;

namespace Gnomeshade.WebApi.Models.Owners;

/// <summary>A group of resources.</summary>
[PublicAPI]
public sealed record Owner
{
	/// <summary>The id of the owner.</summary>
	public Guid Id { get; set; }
}
