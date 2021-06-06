// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU General Public License 3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Data;
using System.IdentityModel.Tokens.Jwt;

using AutoMapper;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Npgsql;
using Npgsql.Logging;

using Tracking.Finance.Data.Identity;
using Tracking.Finance.Data.Models;
using Tracking.Finance.Data.Repositories;
using Tracking.Finance.Interfaces.WebApi.Configuration;
using Tracking.Finance.Interfaces.WebApi.v1_0.Authentication;
using Tracking.Finance.Interfaces.WebApi.v1_0.OpenApi;
using Tracking.Finance.Interfaces.WebApi.v1_0.Transactions;

namespace Tracking.Finance.Interfaces.WebApi
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

			NpgsqlLogManager.Provider = new ConsoleLoggingProvider(NpgsqlLogLevel.Debug, true);
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
			services.AddOptions<JwtOptions>(Configuration);

			services.AddControllers();
			services.AddApiVersioning();

			services.AddIdentityContext(Configuration);

			services
				.AddTransient<JwtSecurityTokenHandler>()
				.AddAuthentication(Options.Authentication)
				.AddJwtBearer(options => Options.JwtBearer(options, Configuration));

			services
				.AddTransient<IDbConnection>(provider => new NpgsqlConnection(Configuration.GetConnectionString("FinanceDb")))
				.AddTransient<TransactionRepository>()
				.AddTransient<TransactionItemRepository>()
				.AddTransient<UserRepository>();

			services.AddSingleton<AutoMapper.IConfigurationProvider>(provider => new MapperConfiguration(options =>
			{
				options.CreateMap<RegistrationModel, ApplicationUser>();
				options.CreateMap<ApplicationUser, UserModel>();

				options.CreateMap<TransactionCreationModel, Transaction>();
				options.CreateMap<Transaction, TransactionModel>();
				options.CreateMap<TransactionItemCreationModel, TransactionItem>();
				options.CreateMap<TransactionItem, TransactionItemModel>();
			}));
			services.AddTransient<Mapper>();

			services.AddSwaggerGen(Options.SwaggerGen);
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
