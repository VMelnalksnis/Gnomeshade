// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Microsoft.AspNetCore.Mvc;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Gnomeshade.Interfaces.WebApi.OpenApi;

/// <summary>
/// A filter that specifies that the action returns <see cref="Status409Conflict"/> with <see cref="ProblemDetails"/>.
/// </summary>
public sealed class ProducesStatus409ConflictAttribute : ProducesResponseTypeAttribute
{
	/// <summary>
	/// Initializes a new instance of the <see cref="ProducesStatus409ConflictAttribute"/> class.
	/// </summary>
	public ProducesStatus409ConflictAttribute()
		: base(typeof(ProblemDetails), Status409Conflict)
	{
	}
}
