// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

using Gnomeshade.WebApi.Configuration.Options;
using Gnomeshade.WebApi.V1.Authorization;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.Net.Http.Headers;

using SameSiteMode = Microsoft.AspNetCore.Http.SameSiteMode;

namespace Gnomeshade.WebApi.Configuration;

internal static class AuthConfiguration
{
	private const string DefaultScheme = "UNKNOWN";
	private static readonly JwtSecurityTokenHandler _tokenHandler = new();

	internal static IServiceCollection AddAuthenticationAndAuthorization(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		IdentityModelEventSource.ShowPII = true;
		services.AddTransient<JwtSecurityTokenHandler>();

		var jwtOptionsDefined = configuration.GetValidIfDefined<JwtOptions>(out var jwtOptions);
		var authenticationSchemes = new List<string>();
		if (jwtOptionsDefined)
		{
			authenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
		}

		var providerSection = configuration.GetSection(OidcProviderOptions.OidcProviderSectionName);
		var providerNames = providerSection.GetChildren().Select(section => section.Key).ToList();

		authenticationSchemes.AddRange(providerNames);

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
					options.DefaultScheme = DefaultScheme;
					options.DefaultAuthenticateScheme = DefaultScheme;
					options.DefaultChallengeScheme = DefaultScheme;
				});

		if (jwtOptionsDefined)
		{
			services.AddValidatedOptions<JwtOptions>(configuration);
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

				options.ForwardDefaultSelector = context => ForwardDefaultSelector(context, authenticationSchemes);
			});
		}
		else
		{
			authenticationBuilder.AddJwtBearer(options =>
				options.ForwardDefaultSelector = context => ForwardDefaultSelector(context, authenticationSchemes));
		}

		foreach (var providerName in providerNames)
		{
			var providerOptions = providerSection.GetValid<OidcProviderOptions>(providerName);
			authenticationBuilder.AddJwtBearer(providerName, options =>
			{
				options.Authority = providerOptions.ServerRealm.AbsoluteUri;
				options.MetadataAddress = providerOptions.Metadata.AbsoluteUri;
				options.Audience = providerOptions.ClientId;
				options.RequireHttpsMetadata = providerOptions.RequireHttpsMetadata;
				options.SaveToken = true;

				options.TokenValidationParameters = new()
				{
					ValidateIssuer = true,
					ValidateAudience = true,
					ValidateLifetime = true,
					ValidateIssuerSigningKey = true,
					ValidAudience = providerOptions.ClientId,
					ClockSkew = TimeSpan.Zero,
					AuthenticationType = providerName,
				};
			});

			var displayName = providerOptions.DisplayName ?? providerName;
			authenticationBuilder.AddOpenIdConnect($"{providerName}_oidc", displayName, options =>
			{
				options.SignInScheme = IdentityConstants.ExternalScheme;
				options.Authority = providerOptions.ServerRealm.AbsoluteUri;
				options.ClientId = providerOptions.ClientId;
				options.ClientSecret = providerOptions.ClientSecret;
				options.MetadataAddress = providerOptions.Metadata.AbsoluteUri;
				options.RequireHttpsMetadata = providerOptions.RequireHttpsMetadata;
				options.GetClaimsFromUserInfoEndpoint = true;
				options.Scope.Add("openid");
				options.Scope.Add("profile");
				options.SaveTokens = true;
				options.ResponseType = OpenIdConnectResponseType.Code;
				options.NonceCookie.SameSite = SameSiteMode.Unspecified;
				options.CorrelationCookie.SameSite = SameSiteMode.Unspecified;
				options.AuthenticationMethod = OpenIdConnectRedirectBehavior.RedirectGet;
				options.SaveTokens = true;
				options.TokenValidationParameters = new()
				{
					NameClaimType = "name",
					RoleClaimType = ClaimTypes.Role,
					ValidateIssuer = true,
					ValidateAudience = true,
					ValidateLifetime = true,
					ValidateIssuerSigningKey = true,
					ValidAudience = providerOptions.ClientId,
					ClockSkew = TimeSpan.Zero,
					AuthenticationType = providerName,
				};
			});
		}

		return services;
	}

	private static string? ForwardDefaultSelector(HttpContext context, IEnumerable<string> authenticationSchemes)
	{
		if (context.Request.Cookies.Keys.Any(key => key.EndsWith(IdentityConstants.ApplicationScheme)))
		{
			return IdentityConstants.ApplicationScheme;
		}

		string? authorization = context.Request.Headers[HeaderNames.Authorization];
		if (string.IsNullOrWhiteSpace(authorization) ||
			!authorization.StartsWith(JwtBearerDefaults.AuthenticationScheme, StringComparison.OrdinalIgnoreCase))
		{
			return null;
		}

		var token = authorization[JwtBearerDefaults.AuthenticationScheme.Length..];

		if (!_tokenHandler.CanReadToken(token))
		{
			return null;
		}

		var jwtToken = _tokenHandler.ReadToken(token);
		return authenticationSchemes.Contains(jwtToken.Issuer)
			? jwtToken.Issuer
			: null;
	}
}
