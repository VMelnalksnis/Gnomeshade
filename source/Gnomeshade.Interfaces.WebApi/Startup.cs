// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Data;
using System.IdentityModel.Tokens.Jwt;

using AutoMapper;

using Gnomeshade.Core.Imports.Fidavista;
using Gnomeshade.Data;
using Gnomeshade.Data.Identity;
using Gnomeshade.Data.Repositories;
using Gnomeshade.Interfaces.WebApi.Configuration;
using Gnomeshade.Interfaces.WebApi.V1_0;
using Gnomeshade.Interfaces.WebApi.V1_0.Authentication;
using Gnomeshade.Interfaces.WebApi.V1_0.OpenApi;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Npgsql;
using Npgsql.Logging;

namespace Gnomeshade.Interfaces.WebApi
{
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

			if (NpgsqlLogManager.Provider is null)
			{
				NpgsqlLogManager.Provider = new ConsoleLoggingProvider(NpgsqlLogLevel.Debug, true);
				NpgsqlLogManager.IsParameterLoggingEnabled = true;
			}

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
			services.AddOptions<JwtOptions>(Configuration);

			services.AddControllers().AddControllersAsServices();
			services.AddApiVersioning();

			services.AddIdentityContext(builder => builder.ConfigureIdentityContext(Configuration));

			services
				.AddTransient<JwtSecurityTokenHandler>()
				.AddAuthentication(Options.Authentication)
				.AddJwtBearer(options => Options.JwtBearer(options, Configuration));

			services
				.AddTransient<IDbConnection>(_ => new NpgsqlConnection(Configuration.GetConnectionString("FinanceDb")))
				.AddTransient<OwnerRepository>()
				.AddTransient<OwnershipRepository>()
				.AddTransient<TransactionRepository>()
				.AddTransient<TransactionItemRepository>()
				.AddTransient<UserRepository>()
				.AddTransient<AccountRepository>()
				.AddTransient<AccountInCurrencyRepository>()
				.AddTransient<CurrencyRepository>()
				.AddTransient<ProductRepository>()
				.AddTransient<UnitRepository>()
				.AddTransient<AccountUnitOfWork>()
				.AddTransient<TransactionUnitOfWork>();

			services
				.AddTransient<Mapper>()
				.AddSingleton<AutoMapper.IConfigurationProvider>(_ => new MapperConfiguration(options =>
				{
					options.CreateMapsForV1_0();
				}));

			services.AddSwaggerGen(Options.SwaggerGen);

			services.AddTransient<FidavistaReader>();
		}

		/// <summary>
		/// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		/// </summary>
		/// <param name="application">Application builder used to configure the HTTP request pipeline.</param>
		/// <param name="environment">The current application environment.</param>
		public void Configure(IApplicationBuilder application, IWebHostEnvironment environment)
		{
			if (environment.IsDevelopment())
			{
				application.UseDeveloperExceptionPage();
			}

			application.UseRouting();

			application.UseAuthentication();
			application.UseAuthorization();

			application.UseEndpoints(endpoints => endpoints.MapControllers());

			application.UseSwagger();
			application.UseSwaggerUI(options => options.SwaggerEndpointV1_0());
		}
	}
}
