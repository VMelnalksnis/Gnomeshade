// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

using Gnomeshade.WebApi.OpenApi;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

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
			options.SchemaFilter<InstantSchemaFilter>();
			options.OperationFilter<ValidationProblemDetailsFilter>();
			options.OperationFilter<InternalServerErrorOperationFilter>();
			options.OperationFilter<UnauthorizedOperationFilter>();

			IncludeXmlDocumentation(options);
			AddSecurity(options);

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

	/// <summary>
	/// Swagger does not handle inheritdoc tags,
	/// so need to replace inheritdoc with the actual documentation before passing it to swagger.
	/// </summary>
	private static void IncludeXmlDocumentation(SwaggerGenOptions options)
	{
		var xmlPaths = new[]
		{
			"Gnomeshade.WebApi.xml",
			"Gnomeshade.WebApi.Models.xml",
			"Gnomeshade.WebApi.Client.xml",
		};

		var xmlDocuments = xmlPaths
			.Select(xmlPath => XDocument.Load(File.OpenRead(Path.Combine(AppContext.BaseDirectory, xmlPath))))
			.ToList();

		var documentedMembers = xmlDocuments
			.Select(xmlDoc => xmlDoc.XPathSelectElements("/doc/members/member[@name and not(inheritdoc)]"))
			.SelectMany(memberElementList => memberElementList)
			.ToDictionary(memberElement => memberElement.Attribute("name")!.Value);

		foreach (var document in xmlDocuments)
		{
			var membersWithInheritedDocumentation =
				document.XPathSelectElements("/doc/members/member[inheritdoc[@cref]]");
			foreach (var member in membersWithInheritedDocumentation)
			{
				var inheritedElement = member.Element("inheritdoc")!;

				var cref = inheritedElement.Attribute("cref")!.Value;
				if (documentedMembers.TryGetValue(cref, out var realDocMember))
				{
					inheritedElement.Parent!.ReplaceNodes(realDocMember.Nodes());
				}
			}

			options.IncludeXmlComments(() => new(document.CreateReader()));
		}
	}

	private static void AddSecurity(SwaggerGenOptions options)
	{
		const string jwtSecurityDefinition = "JWT";
		options.AddSecurityDefinition(jwtSecurityDefinition, new()
		{
			Description = "JWT Authorization header using the Bearer scheme.",
			BearerFormat = "JWT",
			Scheme = Schemes.Bearer,
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
