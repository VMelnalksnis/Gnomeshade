// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Net;
using System.Net.Http;
using System.Text.Json.Serialization.Metadata;

using AutoMapper;

using Gnomeshade.Data;
using Gnomeshade.Data.PostgreSQL;
using Gnomeshade.Data.Sqlite;
using Gnomeshade.WebApi.Configuration;
using Gnomeshade.WebApi.Configuration.Options;
using Gnomeshade.WebApi.Configuration.StartupFilters;
using Gnomeshade.WebApi.Configuration.Swagger;
using Gnomeshade.WebApi.HealthChecks;
using Gnomeshade.WebApi.Models;
using Gnomeshade.WebApi.Services;
using Gnomeshade.WebApi.V1;
using Gnomeshade.WebApi.V1.Importing;
using Gnomeshade.WebApi.V2;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using NodaTime;
using NodaTime.Serialization.SystemTextJson;

using Polly;

using VMelnalksnis.NordigenDotNet.DependencyInjection;
using VMelnalksnis.PaperlessDotNet.DependencyInjection;

namespace Gnomeshade.WebApi;

/// <summary>Configures services and HTTP request pipeline.</summary>
public class Startup
{
	private readonly IConfiguration _configuration;

	/// <summary>Initializes a new instance of the <see cref="Startup"/> class.</summary>
	/// <param name="configuration">Configuration for configuring services and request pipeline.</param>
	public Startup(IConfiguration configuration)
	{
		_configuration = configuration;
	}

	/// <summary>This method gets called by the runtime. Use this method to add services to the container.</summary>
	/// <param name="services">Service collection to which to add services to.</param>
	public void ConfigureServices(IServiceCollection services)
	{
		services
			.AddSingleton<IClock>(SystemClock.Instance)
			.AddSingleton(DateTimeZoneProviders.Tzdb);

		services
			.AddControllersWithViews()
			.AddJsonOptions(options =>
			{
				options.JsonSerializerOptions.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
				options.JsonSerializerOptions.Converters.Add(NodaConverters.InstantConverter);
				options.JsonSerializerOptions.TypeInfoResolver = JsonTypeInfoResolver.Combine(
					new GnomeshadeSerializerContext(),
					new DefaultJsonTypeInfoResolver());
			});

		services.AddRazorPages(options =>
		{
			options.Conventions.AuthorizeAreaFolder("Admin", "/", Policies.Administrator);
		});

		services.AddSingleton<ApplicationVersionService>();

		services
			.AddRepositories()
			.AddTransient<IStartupFilter, DatabaseMigrationStartupFilter>()
			.AddValidatedOptions<AdminOptions>(_configuration)
			.AddTransient<IStartupFilter, AdminUserStartupFilter>();

		var databaseProvider = DatabaseProvider.FromName(_configuration.GetValid<DatabaseOptions>().Provider, true);
		_ = databaseProvider switch
		{
			_ when databaseProvider == DatabaseProvider.PostgreSQL => services.AddPostgreSQL(_configuration),
			_ when databaseProvider == DatabaseProvider.Sqlite => services.AddSqlite(_configuration),
			_ => throw new ArgumentOutOfRangeException(nameof(databaseProvider), databaseProvider, "Unsupported database provider"),
		};

		services
			.AddIdentity()
			.AddIdentityStores()
			.AddDefaultTokenProviders();

		services.AddAuthenticationAndAuthorization(_configuration);

		services
			.AddTransient<Mapper>()
			.AddSingleton<AutoMapper.IConfigurationProvider>(_ =>
			{
				var config = new MapperConfiguration(options =>
				{
					options.AllowNullCollections = true;
					options.CreateMapsForV1_0();
					options.CreateMapsForV2_0();
				});
				config.CompileMappings();
				return config;
			});

		services
			.AddGnomeshadeApiVersioning()
			.AddGnomeshadeApiExplorer()
			.AddGnomeshadeHealthChecks(_configuration)
			.AddGnomeshadeOpenTelemetry(_configuration);

		services.AddV1ImportingServices();
		services
			.AddNordigenDotNet(_configuration)
			.AddPolicyHandler(Policy<HttpResponseMessage>
				.HandleResult(message => message.StatusCode is HttpStatusCode.TooManyRequests)
				.WaitAndRetryAsync(5, GetExponentialDelayWithJitter))
			.AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(3, GetExponentialDelayWithJitter))
			.AddTransientHttpErrorPolicy(builder => builder.CircuitBreakerAsync(10, TimeSpan.FromMinutes(5)));

		services.AddPaperlessDotNet(_configuration);
	}

	/// <summary>This method gets called by the runtime. Use this method to configure the HTTP request pipeline.</summary>
	/// <param name="application">Application builder used to configure the HTTP request pipeline.</param>
	/// <param name="environment">The current application environment.</param>
	public void Configure(IApplicationBuilder application, IWebHostEnvironment environment)
	{
		if (environment.IsDevelopment())
		{
			application.UseDeveloperExceptionPage();
		}

		application.UseStaticFiles();

		application.UseRouting();

		application.UseAuthentication();
		application.UseAuthorization();

		application.UseEndpoints(builder =>
		{
			builder.MapRazorPages().RequireAuthorization(Policies.Identity);
			builder.MapControllers().RequireAuthorization(Policies.ApplicationUser);
		});

		application.UseGnomeshadeApiExplorer();
	}

	private static TimeSpan GetExponentialDelayWithJitter(int retry)
	{
		return TimeSpan.FromSeconds(Math.Pow(2, retry)) + TimeSpan.FromMilliseconds(Random.Shared.Next(0, 1000));
	}
}
