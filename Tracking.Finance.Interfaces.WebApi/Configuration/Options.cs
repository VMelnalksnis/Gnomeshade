// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU General Public License 3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

using Tracking.Finance.Interfaces.WebApi.OpenApi;
using Tracking.Finance.Interfaces.WebApi.v1_0.Authentication;
using Tracking.Finance.Interfaces.WebApi.v1_0.OpenApi;

namespace Tracking.Finance.Interfaces.WebApi.Configuration
{
	public static class Options
	{
		public static void Authentication(AuthenticationOptions options)
		{
			options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
			options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
		}

		public static void JwtBearer(JwtBearerOptions options, IConfiguration configuration)
		{
			var jwtOptions = configuration.GetValid<JwtOptions>();

			options.ClaimsIssuer = jwtOptions.ValidIssuer;
			options.Audience = jwtOptions.ValidAudience;
			options.SaveToken = true;
			options.RequireHttpsMetadata = true;
			options.TokenValidationParameters = new TokenValidationParameters
			{
				ValidateIssuer = true,
				ValidateAudience = true,
				ValidateLifetime = true,
				ValidateIssuerSigningKey = true,
				ValidAudience = jwtOptions.ValidAudience,
				ValidIssuer = jwtOptions.ValidIssuer,
				IssuerSigningKey = jwtOptions.SecurityKey,
			};
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

			// var xmlDocumentationFilepath = Path.Combine(AppContext.BaseDirectory, "Tracking.Finance.Interfaces.WebApi.xml");
			// options.IncludeXmlComments(xmlDocumentationFilepath, true);
			options.EnableAnnotations();

			const string jwtSecurityDefinition = "JWT";
			options.AddSecurityDefinition(jwtSecurityDefinition, new OpenApiSecurityScheme
			{
				Description = "JWT Authorization header using the Bearer scheme.",
				BearerFormat = "JWT",
				Scheme = JwtBearerDefaults.AuthenticationScheme,
				In = ParameterLocation.Header,
				Type = SecuritySchemeType.Http,
			});

			options.AddSecurityRequirement(new OpenApiSecurityRequirement
			{
				{
					new OpenApiSecurityScheme
					{
						Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = jwtSecurityDefinition },
					},
					new List<string>()
				},
			});
		}
	}
}
