// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;

using Gnomeshade.Interfaces.WebApi.OpenApi;
using Gnomeshade.Interfaces.WebApi.V1_0.Authentication;
using Gnomeshade.Interfaces.WebApi.V1_0.Authorization;
using Gnomeshade.Interfaces.WebApi.V1_0.OpenApi;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace Gnomeshade.Interfaces.WebApi.Configuration
{
	public static class Options
	{
		public static void ConfigurePolicies(this AuthorizationOptions authorizationOptions)
		{
			authorizationOptions.DefaultPolicy = new AuthorizationPolicyBuilder()
				.RequireAuthenticatedUser()
				.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme, "keycloak")
				.Build();

			authorizationOptions.AddPolicy(
				AuthorizeApplicationUserAttribute.PolicyName,
				builder => builder
					.AddRequirements(new ApplicationUserRequirement()));
		}

		public static void SetSchemes(this AuthenticationOptions authenticationOptions)
		{
			authenticationOptions.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
			authenticationOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
			authenticationOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
		}

		/// <summary>
		/// Adds JWT bearer authentication from sources defined in <paramref name="configuration"/>.
		/// </summary>
		/// <param name="authenticationBuilder">The authentication builder to configure.</param>
		/// <param name="configuration">Configuration containing JWT source definitions.</param>
		/// <returns>The <paramref name="authenticationBuilder"/> for chaining calls.</returns>
		public static AuthenticationBuilder AddJwtBearerAuthentication(
			this AuthenticationBuilder authenticationBuilder,
			IConfiguration configuration)
		{
			return
				authenticationBuilder
					.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
					{
						var jwtOptions = configuration.GetValid<JwtOptions>();

						options.ClaimsIssuer = jwtOptions.ValidIssuer;
						options.Audience = jwtOptions.ValidAudience;
						options.SaveToken = true;
						options.RequireHttpsMetadata = true;
						options.TokenValidationParameters = new()
						{
							ValidateIssuer = true,
							ValidateAudience = true,
							ValidateLifetime = true,
							ValidateIssuerSigningKey = true,
							ValidAudience = jwtOptions.ValidAudience,
							ValidIssuer = jwtOptions.ValidIssuer,
							IssuerSigningKey = jwtOptions.SecurityKey,
							ClockSkew = TimeSpan.Zero,
						};
					})
					.AddJwtBearer("keycloak", options =>
					{
						var keycloakOptions = configuration.GetValid<KeycloakOptions>();

						options.Authority = keycloakOptions.ServerRealm.AbsoluteUri;
						options.MetadataAddress = keycloakOptions.Metadata.AbsoluteUri;
						options.Audience = keycloakOptions.ClientId;
						options.RequireHttpsMetadata = false;
						options.SaveToken = true;

						options.TokenValidationParameters = new()
						{
							ValidateIssuer = true,
							ValidateAudience = true,
							ValidateLifetime = true,
							ValidateIssuerSigningKey = true,
							ValidAudience = keycloakOptions.ClientId,
							ClockSkew = TimeSpan.Zero,
							AuthenticationType = "keycloak",
						};
					});
		}

		public static void SwaggerGen(SwaggerGenOptions options)
		{
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
			var modelDocumentationFilepath =
				Path.Combine(AppContext.BaseDirectory, "Gnomeshade.Interfaces.WebApi.Models.xml");
			options.IncludeXmlComments(modelDocumentationFilepath, true);
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
}
