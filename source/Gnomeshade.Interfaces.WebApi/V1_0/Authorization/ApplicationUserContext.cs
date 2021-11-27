// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Gnomeshade.Data.Entities;

namespace Gnomeshade.Interfaces.WebApi.V1_0.Authorization;

/// <summary>
/// Contains information about the current application user.
/// </summary>
public sealed class ApplicationUserContext
{
	/// <summary>
	/// Gets or sets the <see cref="UserEntity"/> associated with the executing action.
	/// </summary>
	public UserEntity User { get; set; } = null!;
}
