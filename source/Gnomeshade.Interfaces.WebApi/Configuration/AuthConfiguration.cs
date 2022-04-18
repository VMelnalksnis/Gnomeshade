// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

using Gnomeshade.Interfaces.WebApi.V1_0.Authentication;
using Gnomeshade.Interfaces.WebApi.V1_0.Authorization;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;

namespace Gnomeshade.Interfaces.WebApi.Configuration;

internal static class AuthConfiguration
{
	private const string _oAuth2Providers = "OAuth2Providers";

	internal static IServiceCollection AddAuthenticationAndAuthorization(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		IdentityModelEventSource.ShowPII = true;

		var jwtOptionsDefined = configuration.GetValidIfDefined<JwtOptions>(out var jwtOptions);
		var authenticationSchemes = new List<string>();
		if (jwtOptionsDefined)
		{
			authenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
		}

		var providerSection = configuration.GetSection(_oAuth2Providers);
		var providerNames = providerSection.GetChildren().Select(section => section.Key).ToList();

		authenticationSchemes.AddRange(providerNames);
		if (jwtOptionsDefined)
		{
			services.AddTransient<JwtSecurityTokenHandler>();
		}

		var authenticationBuilder =
			services
				.AddAuthorization(options =>
				{
					options.DefaultPolicy = new AuthorizationPolicyBuilder()
						.RequireAuthenticatedUser()
						.AddAuthenticationSchemes(authenticationSchemes.ToArray())
						.Build();

					options.AddPolicy(
						AuthorizeApplicationUserAttribute.PolicyName,
						policy => policy.AddRequirements(new ApplicationUserRequirement()));
				})
				.AddScoped<IAuthorizationHandler, ApplicationUserHandler>()
				.AddScoped<ApplicationUserContext>()
				.AddAuthentication(options =>
				{
					var defaultScheme = authenticationSchemes.First();

					options.DefaultScheme = defaultScheme;
					options.DefaultAuthenticateScheme = defaultScheme;
					options.DefaultChallengeScheme = defaultScheme;
				});

		if (jwtOptionsDefined)
		{
			authenticationBuilder.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
			{
				options.ClaimsIssuer = jwtOptions!.ValidIssuer;
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
			});
		}

		foreach (var providerName in providerNames)
		{
			var keycloakOptions = providerSection.GetValid<KeycloakOptions>();
			authenticationBuilder.AddJwtBearer(providerName, options =>
			{
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
					AuthenticationType = providerName,
				};
			});
		}

		return services;
	}
}
