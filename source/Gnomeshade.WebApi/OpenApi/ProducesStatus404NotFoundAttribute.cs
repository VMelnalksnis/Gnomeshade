// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Microsoft.AspNetCore.Mvc;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Gnomeshade.WebApi.OpenApi;

/// <summary>
/// A filter that specifies that the action returns <see cref="Status404NotFound"/> with <see cref="ProblemDetails"/>.
/// </summary>
public sealed class ProducesStatus404NotFoundAttribute : ProducesResponseTypeAttribute
{
	/// <summary>
	/// Initializes a new instance of the <see cref="ProducesStatus404NotFoundAttribute"/> class.
	/// </summary>
	public ProducesStatus404NotFoundAttribute()
		: base(typeof(ProblemDetails), Status404NotFound)
	{
	}
}
