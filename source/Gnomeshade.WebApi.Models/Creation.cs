// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

namespace Gnomeshade.WebApi.Models;

/// <summary>Base class for all resource creation models.</summary>
public abstract record Creation
{
	/// <summary>The id of the owner of the resource.</summary>
	public Guid? OwnerId { get; init; }
}
