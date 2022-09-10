// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using JetBrains.Annotations;

using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace Gnomeshade.WebApi.OpenApi;

/// <summary> Adds <see cref="StatusCodes.Status500InternalServerError"/> response to all operations. </summary>
[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
internal sealed class InternalServerErrorOperationFilter : IOperationFilter
{
	/// <inheritdoc/>
	public void Apply(OpenApiOperation operation, OperationFilterContext context)
	{
		if (operation.Responses.ContainsKey("500"))
		{
			return;
		}

		operation.Responses.Add("500", new() { Description = "Internal Server Error" });
	}
}
