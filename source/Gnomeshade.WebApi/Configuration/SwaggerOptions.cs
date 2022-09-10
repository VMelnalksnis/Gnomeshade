// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Gnomeshade.WebApi.OpenApi;
using Gnomeshade.WebApi.V1.OpenApi;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace Gnomeshade.WebApi.Configuration;

internal static class SwaggerOptions
{
	internal static void SwaggerGen(SwaggerGenOptions options)
	{
		options.SupportNonNullableReferenceTypes();
		options.SwaggerDocV1_0();

		options.DocumentFilter<ApiVersioningFilter>();
		options.OperationFilter<ApiVersioningFilter>();

		options.SchemaFilter<ValidationProblemDetailsFilter>();
		options.SchemaFilter<ValidationProblemDetailsSchemaFilter>();
		options.OperationFilter<ValidationProblemDetailsFilter>();

		options.OperationFilter<InternalServerErrorOperationFilter>();
		options.OperationFilter<UnauthorizedOperationFilter>();

		var xmlDocumentationFilepath = Path.Combine(AppContext.BaseDirectory, "Gnomeshade.WebApi.xml");
		options.IncludeXmlComments(xmlDocumentationFilepath);
		var modelDocumentationFilepath = Path.Combine(AppContext.BaseDirectory, "Gnomeshade.WebApi.Models.xml");
		options.IncludeXmlComments(modelDocumentationFilepath);
		var clientDocumentationFilepath = Path.Combine(AppContext.BaseDirectory, "Gnomeshade.WebApi.Client.xml");
		options.IncludeXmlComments(clientDocumentationFilepath);
		options.EnableAnnotations();

		const string jwtSecurityDefinition = "JWT";
		options.AddSecurityDefinition(jwtSecurityDefinition, new()
		{
			Description = "JWT Authorization header using the Bearer scheme.",
			BearerFormat = "JWT",
			Scheme = JwtBearerDefaults.AuthenticationScheme,
			In = ParameterLocation.Header,
			Type = SecuritySchemeType.Http,
		});

		options.AddSecurityRequirement(new()
		{
			{
				new()
				{
					Reference = new() { Type = ReferenceType.SecurityScheme, Id = jwtSecurityDefinition },
				},
				new List<string>()
			},
		});

		options.ResolveConflictingActions(enumerable =>
			enumerable.OrderBy(description => description.ParameterDescriptions.Count).Last());
	}
}
