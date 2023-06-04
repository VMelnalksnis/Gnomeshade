// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using AspNet.Security.OAuth.GitHub;

using Gnomeshade.WebApi.Configuration.Options;
using Gnomeshade.WebApi.V1.Authorization;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.Net.Http.Headers;

using SameSiteMode = Microsoft.AspNetCore.Http.SameSiteMode;

namespace Gnomeshade.WebApi.Configuration;

internal static class AuthConfiguration
{
	private static readonly JwtSecurityTokenHandler _tokenHandler = new();
	private static readonly string[] _supportedOAuthProviders =
	{
		GitHubAuthenticationDefaults.AuthenticationScheme,
	};

	internal static IServiceCollection AddAuthenticationAndAuthorization(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		services.AddTransient<JwtSecurityTokenHandler>();

		var issuers = new Dictionary<string, string>();
		var authenticationSchemes = new List<string>();

		var jwtOptionsDefined = configuration.GetValidIfDefined<JwtOptions>(out var jwtOptions);
		if (jwtOptionsDefined)
		{
			issuers.Add(jwtOptions!.ValidIssuer, JwtBearerDefaults.AuthenticationScheme);
			authenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
		}

		var oidcProviderSection = configuration.GetSection(OidcProviderOptions.OidcProviderSectionName);
		var oidcProviderNames = oidcProviderSection.GetChildren().Select(section => section.Key).ToArray();

		var oauthProviderSection = configuration.GetSection(OAuthProviderOptions.ProviderSectionName);
		var oauthProviderNames = oauthProviderSection
			.GetChildren()
			.Select(section => section.Key)
			.Where(key => _supportedOAuthProviders.Contains(key))
			.ToArray();

		authenticationSchemes.AddRange(oidcProviderNames);
		authenticationSchemes.AddRange(oauthProviderNames);

		var authenticationBuilder =
			services
				.AddAuthorization(options =>
				{
					options.DefaultPolicy = new AuthorizationPolicyBuilder()
						.RequireAuthenticatedUser()
						.AddAuthenticationSchemes(authenticationSchemes.ToArray())
						.Build();

					options.AddPolicy(
						Policies.ApplicationUser,
						policy => policy.AddRequirements(new ApplicationUserRequirement()));

					options.AddPolicy(
						Policies.Administrator,
						policy => policy
							.AddRequirements(new ApplicationUserRequirement())
							.AddRequirements(new AdministratorRoleRequirement()));
				})
				.AddScoped<IAuthorizationHandler, ApplicationUserHandler>()
				.AddScoped<ApplicationUserContext>()
				.AddAuthentication();

		if (jwtOptionsDefined)
		{
			services.AddValidatedOptions<JwtOptions>(configuration);
			authenticationBuilder.AddJwtBearer(options =>
			{
				options.ClaimsIssuer = jwtOptions!.ValidIssuer;
				options.Audience = jwtOptions.ValidAudience;
				options.SaveToken = true;
				options.TokenValidationParameters = new()
				{
					ValidateIssuerSigningKey = true,
					ValidAudience = jwtOptions.ValidAudience,
					ValidIssuer = jwtOptions.ValidIssuer,
					IssuerSigningKey = jwtOptions.SecurityKey,
					ClockSkew = TimeSpan.Zero,
				};

				options.ForwardDefaultSelector = context => ForwardDefaultSelector(context, issuers, JwtBearerDefaults.AuthenticationScheme);
			});
		}
		else
		{
			authenticationBuilder.AddJwtBearer(options => options.ForwardDefaultSelector =
				context => ForwardDefaultSelector(context, issuers, JwtBearerDefaults.AuthenticationScheme));
		}

		foreach (var providerName in oidcProviderNames)
		{
			var providerOptions = oidcProviderSection.GetValid<OidcProviderOptions>(providerName);
			issuers.Add(providerOptions.ServerRealm.AbsoluteUri, providerName);
			authenticationBuilder.AddJwtBearer(providerName, options =>
			{
				options.Authority = providerOptions.ServerRealm.AbsoluteUri;
				options.MetadataAddress = providerOptions.Metadata.AbsoluteUri;
				options.Audience = providerOptions.ClientId;
				options.RequireHttpsMetadata = providerOptions.RequireHttpsMetadata;
				options.SaveToken = true;

				options.TokenValidationParameters = new()
				{
					ValidateIssuerSigningKey = true,
					ValidAudience = providerOptions.ClientId,
					ClockSkew = TimeSpan.Zero,
					AuthenticationType = providerName,
				};

				options.ForwardDefaultSelector = context => ForwardDefaultSelector(context, issuers, providerName);
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
					ValidateIssuerSigningKey = true,
					ValidAudience = providerOptions.ClientId,
					ClockSkew = TimeSpan.Zero,
					AuthenticationType = providerName,
				};
			});
		}

		foreach (var providerName in oauthProviderNames)
		{
			var providerOptions = oauthProviderSection.GetValid<OAuthProviderOptions>(providerName);
			if (providerName is GitHubAuthenticationDefaults.AuthenticationScheme)
			{
				authenticationBuilder.AddGitHub(options =>
				{
					options.ClientId = providerOptions.ClientId;
					options.ClientSecret = providerOptions.ClientSecret!;
					options.Events.OnRedirectToAuthorizationEndpoint = OnRedirectToAuthorizationEndpoint;
					options.ForwardDefaultSelector = context => ForwardDefaultSelector(context, issuers, providerName);
				});
			}
		}

		return services;
	}

	private static string? ForwardDefaultSelector(HttpContext context, IReadOnlyDictionary<string, string> issuers, string currentScheme)
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

		var token = authorization[(JwtBearerDefaults.AuthenticationScheme.Length + 1)..];
		if (!_tokenHandler.CanReadToken(token))
		{
			return null;
		}

		var jwtToken = _tokenHandler.ReadToken(token);
		return issuers.TryGetValue(jwtToken.Issuer, out var scheme) && scheme != currentScheme
			? scheme
			: null;
	}

	/// <seealso cref="OAuthEvents.OnRedirectToAuthorizationEndpoint"/>
	private static Task OnRedirectToAuthorizationEndpoint(RedirectContext<OAuthOptions> context)
	{
		if (context.Request.Path.StartsWithSegments("/api"))
		{
			context.Response.StatusCode = StatusCodes.Status401Unauthorized;
		}
		else
		{
			context.Response.Redirect(context.RedirectUri);
		}

		return Task.CompletedTask;
	}
}
