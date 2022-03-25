﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using JetBrains.Annotations;

using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace Gnomeshade.Interfaces.WebApi.OpenApi;

/// <summary>Applies missing inforation to <see cref="ValidationProblemDetails"/> schema.</summary>
[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class ValidationProblemDetailsSchemaFilter : SchemaFilter<ValidationProblemDetails>
{
	/// <inheritdoc/>
	protected override void ApplyFilter(OpenApiSchema schema, SchemaFilterContext context)
	{
		schema.Properties["status"].Default = new OpenApiInteger(400);
	}
}
