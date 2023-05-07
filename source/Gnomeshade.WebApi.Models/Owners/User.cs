// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using JetBrains.Annotations;

namespace Gnomeshade.WebApi.Models.Owners;

/// <summary>An application user.</summary>
[PublicAPI]
public sealed record User
{
	/// <summary>The id of the user.</summary>
	public Guid Id { get; set; }
}
