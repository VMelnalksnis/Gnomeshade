// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Data;
using System.IdentityModel.Tokens.Jwt;

using AutoMapper;

using Elastic.Apm.NetCoreAll;

using Gnomeshade.Data;
using Gnomeshade.Data.Identity;
using Gnomeshade.Data.Repositories;
using Gnomeshade.Interfaces.WebApi.Configuration;
using Gnomeshade.Interfaces.WebApi.Logging;
using Gnomeshade.Interfaces.WebApi.V1_0;
using Gnomeshade.Interfaces.WebApi.V1_0.Authentication;
using Gnomeshade.Interfaces.WebApi.V1_0.Authorization;
using Gnomeshade.Interfaces.WebApi.V1_0.Importing;
using Gnomeshade.Interfaces.WebApi.V1_0.OpenApi;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Npgsql;
using Npgsql.Logging;

using Serilog;

namespace Gnomeshade.Interfaces.WebApi;

/// <summary>
/// Configures services and HTTP request pipeline.
/// </summary>
public class Startup
{
	/// <summary>
	/// Initializes a new instance of the <see cref="Startup"/> class.
	/// </summary>
	/// <param name="configuration">Configuration for configuring services and request pipeline.</param>
	public Startup(IConfiguration configuration)
	{
		Configuration = configuration;

		NpgsqlLogManager.Provider = new SerilogNpgsqlLoggingProvider();
		NpgsqlLogManager.IsParameterLoggingEnabled = true;

		using var database = new ApplicationDbContext(Configuration);
		database.Database.EnsureCreated();
	}

	/// <summary>
	/// Gets the configuration used for configuring services and request pipeline.
	/// </summary>
	public IConfiguration Configuration { get; }

	/// <summary>
	/// This method gets called by the runtime. Use this method to add services to the container.
	/// </summary>
	/// <param name="services">Service collection to which to add services to.</param>
	public void ConfigureServices(IServiceCollection services)
	{
		services.AddLogging(builder => builder.AddSerilog());

		services.AddValidatedOptions<JwtOptions>(Configuration);

		services.AddControllers().AddControllersAsServices();
		services.AddApiVersioning();

		services.AddIdentityContext(builder => builder.ConfigureIdentityContext(Configuration));

		services
			.AddAuthorization(options => options.ConfigurePolicies(Configuration))
			.AddScoped<IAuthorizationHandler, ApplicationUserHandler>()
			.AddScoped<ApplicationUserContext>()
			.AddTransient<JwtSecurityTokenHandler>()
			.AddAuthentication(options => options.SetSchemes())
			.AddJwtBearerAuthentication(Configuration);

		services
			.AddScoped<IDbConnection>(_ => new NpgsqlConnection(Configuration.GetConnectionString("FinanceDb")))
			.AddScoped<OwnerRepository>()
			.AddScoped<OwnershipRepository>()
			.AddScoped<TransactionRepository>()
			.AddScoped<TransactionItemRepository>()
			.AddScoped<UserRepository>()
			.AddScoped<AccountRepository>()
			.AddScoped<AccountInCurrencyRepository>()
			.AddScoped<CurrencyRepository>()
			.AddScoped<ProductRepository>()
			.AddScoped<UnitRepository>()
			.AddScoped<CounterpartyRepository>()
			.AddScoped<AccountUnitOfWork>()
			.AddScoped<TagRepository>()
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
	}

	/// <summary>
	/// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
	/// </summary>
	/// <param name="application">Application builder used to configure the HTTP request pipeline.</param>
	/// <param name="environment">The current application environment.</param>
	public void Configure(IApplicationBuilder application, IWebHostEnvironment environment)
	{
		application.UseAllElasticApm(Configuration);

		if (environment.IsDevelopment())
		{
			application.UseDeveloperExceptionPage();
		}

		application.UseSerilogRequestLogging();

		application.UseRouting();

		application.UseAuthentication();
		application.UseAuthorization();

		application.UseEndpoints(builder => builder.MapControllers().RequireAuthorization());

		application.UseSwagger();
		application.UseSwaggerUI(options => options.SwaggerEndpointV1_0());
	}
}
