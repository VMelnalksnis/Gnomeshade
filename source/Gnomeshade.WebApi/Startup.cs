// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Data;

using AutoMapper;

using Dapper;

using Elastic.Apm.AspNetCore;
using Elastic.Apm.AspNetCore.DiagnosticListener;
using Elastic.Apm.DiagnosticSource;
using Elastic.Apm.Elasticsearch;
using Elastic.Apm.EntityFrameworkCore;
using Elastic.Apm.SqlClient;

using Gnomeshade.Data;
using Gnomeshade.Data.Dapper;
using Gnomeshade.Data.Identity;
using Gnomeshade.Data.Migrations;
using Gnomeshade.Data.Repositories;
using Gnomeshade.WebApi.Configuration;
using Gnomeshade.WebApi.HealthChecks;
using Gnomeshade.WebApi.Logging;
using Gnomeshade.WebApi.V1_0;
using Gnomeshade.WebApi.V1_0.Importing;
using Gnomeshade.WebApi.V1_0.OpenApi;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using NodaTime;
using NodaTime.Serialization.SystemTextJson;

using Npgsql;
using Npgsql.Logging;

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

		NpgsqlLogManager.Provider = new SerilogNpgsqlLoggingProvider();
		NpgsqlLogManager.IsParameterLoggingEnabled = true;
		NpgsqlConnection.GlobalTypeMapper.UseNodaTime();
		SqlMapper.AddTypeHandler(typeof(Instant?), new NullableInstantTypeHandler());
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

		services.AddIdentityContext();
		services.AddAuthenticationAndAuthorization(_configuration);

		services
			.AddScoped<IDbConnection>(_ => new NpgsqlConnection(_configuration.GetConnectionString("FinanceDb")))
			.AddScoped<OwnerRepository>()
			.AddScoped<OwnershipRepository>()
			.AddScoped<TransactionRepository>()
			.AddScoped<PurchaseRepository>()
			.AddScoped<TransferRepository>()
			.AddScoped<LoanRepository>()
			.AddScoped<UserRepository>()
			.AddScoped<AccountRepository>()
			.AddScoped<AccountInCurrencyRepository>()
			.AddScoped<CurrencyRepository>()
			.AddScoped<ProductRepository>()
			.AddScoped<UnitRepository>()
			.AddScoped<CounterpartyRepository>()
			.AddScoped<LinkRepository>()
			.AddScoped<AccessRepository>()
			.AddScoped<AccountUnitOfWork>()
			.AddScoped<CategoryRepository>()
			.AddScoped<TransactionUnitOfWork>()
			.AddScoped<UserUnitOfWork>()
			.AddTransient<Iso20022AccountReportReader>();

		services
			.AddTransient<Mapper>()
			.AddSingleton<AutoMapper.IConfigurationProvider>(_ =>
			{
				var config = new MapperConfiguration(options => options.CreateMapsForV1_0());
				config.CompileMappings();
				return config;
			});

		services.AddSwaggerGen(Options.SwaggerGen);

		services
			.AddTransient<DatabaseMigrator>()
			.AddTransient<IStartupFilter, DatabaseMigrationStartupFilter>();

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
