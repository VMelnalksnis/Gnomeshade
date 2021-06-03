using System;
using System.Data;
using System.IO;

using AutoMapper;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Npgsql;
using Npgsql.Logging;

using Tracking.Finance.Data.Models;
using Tracking.Finance.Data.Repositories;
using Tracking.Finance.Interfaces.WebApi.OpenApi;
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
			services
				.AddControllers()
				.ConfigureApiBehaviorOptions(options =>
				{
					var x = options.ClientErrorMapping.Keys;
				});
			services.AddApiVersioning();

			services.AddTransient<IDbConnection>(provider => new NpgsqlConnection(Configuration.GetConnectionString("FinanceDb")));
			NpgsqlLogManager.Provider = new ConsoleLoggingProvider(NpgsqlLogLevel.Debug, true);
			NpgsqlLogManager.IsParameterLoggingEnabled = true;
			services.AddTransient<TransactionRepository>();
			services.AddTransient<TransactionItemRepository>();

			services.AddSingleton<AutoMapper.IConfigurationProvider>(provider => new MapperConfiguration(options =>
			{
				options.CreateMap<TransactionCreationModel, Transaction>();
				options.CreateMap<Transaction, TransactionModel>();
				options.CreateMap<TransactionItemCreationModel, TransactionItem>();
				options.CreateMap<TransactionItem, TransactionItemModel>();
			}));
			services.AddTransient<Mapper>();

			services.AddSwaggerGen(options =>
			{
				options.SwaggerDocV1_0();

				options.DocumentFilter<ApiVersioningFilter>();
				options.OperationFilter<ApiVersioningFilter>();

				options.SchemaFilter<ValidationProblemDetailsFilter>();
				options.SchemaFilter<ValidationProblemDetailsSchemaFilter>();
				options.OperationFilter<ValidationProblemDetailsFilter>();

				options.OperationFilter<InternalServerErrorOperationFilter>();

				// var xmlDocumentationFilepath = Path.Combine(AppContext.BaseDirectory, "Tracking.Finance.Interfaces.WebApi.xml");
				// options.IncludeXmlComments(xmlDocumentationFilepath, true);
				options.EnableAnnotations();
			});
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

			application.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
				endpoints.MapGet("/", async context =>
				{
					await context.Response.WriteAsync("Hello World!");
				});
			});

			application.UseSwagger();
			application.UseSwaggerUI(options =>
			{
				options.SwaggerEndpointV1_0();
			});
		}
	}
}
