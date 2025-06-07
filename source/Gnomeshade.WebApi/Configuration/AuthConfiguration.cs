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
using Gnomeshade.WebApi.V1.Authentication;
using Gnomeshade.WebApi.V1.Authorization;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Gnomeshade.WebApi.Configuration;

internal static class AuthConfiguration
{
	internal const string OidcSuffix = "_oidc";

	private static readonly string[] _supportedOAuthProviders =
	[
		GitHubAuthenticationDefaults.AuthenticationScheme,
	];

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
			issuers.Add(jwtOptions!.ValidIssuer, Schemes.Bearer);
			authenticationSchemes.Add(Schemes.Bearer);
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
					options.AddPolicy(
						Policies.ApplicationUser,
						policy => policy
							.AddAuthenticationSchemes(authenticationSchemes.ToArray())
							.AddRequirements(new ApplicationUserRequirement()));

					options.AddPolicy(
						Policies.Administrator,
						policy => policy
							.AddAuthenticationSchemes(authenticationSchemes.ToArray())
							.AddRequirements(new ApplicationUserRequirement())
							.AddRequirements(new AdministratorRoleRequirement()));

					options.AddPolicy(
						Policies.Identity,
						policy => policy
							.AddAuthenticationSchemes(Schemes.Application)
							.AddRequirements(new ApplicationUserRequirement()));
				})
				.AddScoped<IAuthorizationHandler, ApplicationUserHandler>()
				.AddAuthentication(options =>
				{
					options.DefaultAuthenticateScheme = Schemes.Application;
					options.DefaultChallengeScheme = Schemes.Application;
					options.DefaultSignInScheme = Schemes.External;
				})
				.AddCookie(Schemes.Application, options =>
				{
					options.Cookie.Name = Schemes.Application;
					options.LoginPath = new("/Identity/Account/Login");
					options.LogoutPath = new("/Identity/Account/Logout");
					options.AccessDeniedPath = new("/Identity/Account/AccessDenied");
					options.Events = new()
					{
						OnValidatePrincipal = SecurityStampValidator.ValidatePrincipalAsync,
					};
				})
				.AddCookie(Schemes.External, options =>
				{
					options.Cookie.Name = Schemes.External;
					options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
				})
				.AddCookie(Schemes.TwoFactorRememberMe, options =>
				{
					options.LoginPath = new("/Identity/Account/Login");
					options.LogoutPath = new("/Identity/Account/Logout");
					options.AccessDeniedPath = new("/Identity/Account/AccessDenied");
					options.Cookie.Name = Schemes.TwoFactorRememberMe;
					options.Events = new()
					{
						OnValidatePrincipal = SecurityStampValidator.ValidateAsync<ITwoFactorSecurityStampValidator>,
					};
				})
				.AddCookie(Schemes.TwoFactorUserId, options =>
				{
					options.Cookie.Name = Schemes.TwoFactorUserId;
					options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
				});

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
			});
		}
		else
		{
			authenticationBuilder.AddJwtBearer();
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
			});

			var displayName = providerOptions.DisplayName ?? providerName;
			authenticationBuilder.AddOpenIdConnect(providerName + OidcSuffix, displayName, options =>
			{
				options.SignInScheme = Schemes.External;
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
				options.AuthenticationMethod = OpenIdConnectRedirectBehavior.RedirectGet;
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
				});
			}
		}

		services.AddSingleton<IAuthenticationSchemeProvider, CustomAuthenticationSchemeProvider>(provider =>
		{
			var options = provider.GetRequiredService<IOptions<AuthenticationOptions>>();
			var accessor = provider.GetRequiredService<IHttpContextAccessor>();
			var tokenHandler = provider.GetRequiredService<JwtSecurityTokenHandler>();
			return new(options, accessor, tokenHandler, issuers);
		});

		return services;
	}

	/// <seealso cref="OAuthEvents.OnRedirectToAuthorizationEndpoint"/>
	private static Task OnRedirectToAuthorizationEndpoint(RedirectContext<OAuthOptions> context)
	{
		if (context.Request.IsApiRequest())
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
