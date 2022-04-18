// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

using Gnomeshade.Interfaces.WebApi.V1_0.Authentication;
using Gnomeshade.Interfaces.WebApi.V1_0.Authorization;

using Microsoft.AspNetCore.Authentication;
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
		services
			.AddAuthorization(options => options.ConfigurePolicies(configuration))
			.AddScoped<IAuthorizationHandler, ApplicationUserHandler>()
			.AddScoped<ApplicationUserContext>()
			.AddTransient<JwtSecurityTokenHandler>()
			.AddAuthentication(options => options.SetSchemes())
			.AddJwtBearerAuthentication(configuration);

		return services;
	}

	private static void ConfigurePolicies(
		this AuthorizationOptions authorizationOptions,
		IConfiguration configuration)
	{
		var authenticationSchemes = new List<string>();
		if (configuration.GetChildren().Any(section => section.Key == typeof(JwtOptions).GetSectionName()))
		{
			authenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
		}

		var oauthProviderNames =
			configuration
				.GetSection(_oAuth2Providers)
				.GetChildren()
				.Select(section => section.Key);
		authenticationSchemes.AddRange(oauthProviderNames);

		authorizationOptions.DefaultPolicy = new AuthorizationPolicyBuilder()
			.RequireAuthenticatedUser()
			.AddAuthenticationSchemes(authenticationSchemes.ToArray())
			.Build();

		authorizationOptions.AddPolicy(
			AuthorizeApplicationUserAttribute.PolicyName,
			builder => builder
				.AddRequirements(new ApplicationUserRequirement()));
	}

	private static void SetSchemes(this AuthenticationOptions authenticationOptions)
	{
		authenticationOptions.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
		authenticationOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
		authenticationOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
	}

	private static void AddJwtBearerAuthentication(
		this AuthenticationBuilder authenticationBuilder,
		IConfiguration configuration)
	{
		IdentityModelEventSource.ShowPII = true;
		if (configuration.GetChildren().Any(section => section.Key == typeof(JwtOptions).GetSectionName()))
		{
			authenticationBuilder.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
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
			});
		}

		var providerSection = configuration.GetSection(_oAuth2Providers);
		var providerSectionNames = providerSection.GetChildren().Select(section => section.Key);
		foreach (var providerName in providerSectionNames)
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
	}
}
