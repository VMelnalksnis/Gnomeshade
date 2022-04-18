// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;

using Gnomeshade.Interfaces.WebApi.OpenApi;
using Gnomeshade.Interfaces.WebApi.V1_0.OpenApi;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace Gnomeshade.Interfaces.WebApi.Configuration;

internal static class Options
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

		var xmlDocumentationFilepath = Path.Combine(AppContext.BaseDirectory, "Gnomeshade.Interfaces.WebApi.xml");
		options.IncludeXmlComments(xmlDocumentationFilepath, true);
		var modelDocumentationFilepath = Path.Combine(AppContext.BaseDirectory, "Gnomeshade.Interfaces.WebApi.Models.xml");
		options.IncludeXmlComments(modelDocumentationFilepath, true);
		var clientDocumentationFilepath = Path.Combine(AppContext.BaseDirectory, "Gnomeshade.Interfaces.WebApi.Client.xml");
		options.IncludeXmlComments(clientDocumentationFilepath, true);
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
	}
}
