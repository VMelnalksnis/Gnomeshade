// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Gnomeshade.WebApi.OpenApi;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace Gnomeshade.WebApi.Configuration.Swagger;

internal static class SwaggerConfigurationExtensions
{
	internal static IServiceCollection AddGnomeshadeApiExplorer(this IServiceCollection services) =>
		services.AddSwaggerGen(options =>
		{
			options.SupportNonNullableReferenceTypes();
			options.EnableAnnotations();

			options.SchemaFilter<ValidationProblemDetailsFilter>();
			options.SchemaFilter<ValidationProblemDetailsSchemaFilter>();
			options.OperationFilter<ValidationProblemDetailsFilter>();
			options.OperationFilter<InternalServerErrorOperationFilter>();
			options.OperationFilter<UnauthorizedOperationFilter>();

			var xmlPaths = new[]
			{
				"Gnomeshade.WebApi.xml",
				"Gnomeshade.WebApi.Models.xml",
				"Gnomeshade.WebApi.Client.xml",
			};

			foreach (var xmlPath in xmlPaths)
			{
				options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlPath));
			}

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
		});

	internal static void UseGnomeshadeApiExplorer(this IApplicationBuilder application)
	{
		application.UseSwagger();

		var provider = application.ApplicationServices.GetRequiredService<IApiVersionDescriptionProvider>();
		application.UseSwaggerUI(options =>
		{
			foreach (var versionDescription in provider.ApiVersionDescriptions)
			{
				options.SwaggerEndpoint(
					$"/swagger/{versionDescription.GroupName}/swagger.json",
					versionDescription.ApiVersion.ToString());
			}
		});
	}
}
