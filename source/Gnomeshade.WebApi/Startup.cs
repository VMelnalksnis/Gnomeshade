// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using AutoMapper;

using Elastic.Apm.AspNetCore;
using Elastic.Apm.AspNetCore.DiagnosticListener;
using Elastic.Apm.DiagnosticSource;
using Elastic.Apm.Elasticsearch;
using Elastic.Apm.EntityFrameworkCore;
using Elastic.Apm.SqlClient;

using Gnomeshade.Data;
using Gnomeshade.Data.PostgreSQL;
using Gnomeshade.Data.Sqlite;
using Gnomeshade.WebApi.Configuration;
using Gnomeshade.WebApi.Configuration.Options;
using Gnomeshade.WebApi.HealthChecks;
using Gnomeshade.WebApi.Logging;
using Gnomeshade.WebApi.V1;
using Gnomeshade.WebApi.V1.Importing;
using Gnomeshade.WebApi.V1.OpenApi;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using NodaTime;
using NodaTime.Serialization.SystemTextJson;

using Serilog;

using VMelnalksnis.NordigenDotNet.DependencyInjection;

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
		services.AddLogging(builder => builder.AddSerilog());

		services
			.AddSingleton<IClock>(SystemClock.Instance)
			.AddSingleton(DateTimeZoneProviders.Tzdb);

		services
			.AddControllers()
			.AddControllersAsServices()
			.AddJsonOptions(options =>
			{
				options.JsonSerializerOptions.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
				options.JsonSerializerOptions.Converters.Add(NodaConverters.InstantConverter);
			});

		services.AddApiVersioning();

		services.AddRepositories();
		var databaseProvider = _configuration.GetValid<DatabaseOptions>().Provider;
		_ = databaseProvider switch
		{
			_ when databaseProvider.Equals(DatabaseProvider.PostgreSQL.Name, StringComparison.OrdinalIgnoreCase) => services
				.AddPostgreSQL(_configuration, new SerilogNpgsqlLoggingProvider())
				.AddPostgreSQLIdentity(),

			_ when databaseProvider.Equals(DatabaseProvider.Sqlite.Name, StringComparison.OrdinalIgnoreCase) => services
				.AddSqlite(_configuration)
				.AddSqliteIdentity(),

			_ => throw new ArgumentOutOfRangeException(nameof(databaseProvider), databaseProvider, "Unsupported database provider"),
		};

		services.AddAuthenticationAndAuthorization(_configuration);

		services
			.AddTransient<Iso20022AccountReportReader>();

		services
			.AddTransient<Mapper>()
			.AddSingleton<AutoMapper.IConfigurationProvider>(_ =>
			{
				var config = new MapperConfiguration(options => options.CreateMapsForV1_0());
				config.CompileMappings();
				return config;
			});

		services.AddSwaggerGen(SwaggerOptions.SwaggerGen);

		services.AddGnomeshadeHealthChecks();
		services.AddNordigenDotNet(_configuration);
	}

	/// <summary>This method gets called by the runtime. Use this method to configure the HTTP request pipeline.</summary>
	/// <param name="application">Application builder used to configure the HTTP request pipeline.</param>
	/// <param name="environment">The current application environment.</param>
	public void Configure(IApplicationBuilder application, IWebHostEnvironment environment)
	{
		application.UseElasticApm(
			_configuration,
			new AspNetCoreDiagnosticSubscriber(),
			new HttpDiagnosticsSubscriber(),
			new EfCoreDiagnosticsSubscriber(),
			new SqlClientDiagnosticSubscriber(),
			new ElasticsearchDiagnosticsSubscriber());

		if (environment.IsDevelopment())
		{
			application.UseDeveloperExceptionPage();
		}

		application.UseSerilogRequestLogging();

		application.UseRouting();

		application.UseAuthentication();
		application.UseAuthorization();

		application.UseEndpoints(builder =>
		{
			builder.MapControllers().RequireAuthorization();
			builder.MapHealthChecks("/health").AllowAnonymous();
		});

		application.UseSwagger();
		application.UseSwaggerUI(options => options.SwaggerEndpointV1_0());
	}
}
