using System;
using System.IO;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

using Tracking.Finance.Web.Data;

namespace Tracking.Finance.Web
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
			services
				.AddDbContext<ApplicationDbContext>(options => options.ConfigurePostgres(Configuration))
				.AddDatabaseDeveloperPageExceptionFilter();

			services
				.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
				.AddEntityFrameworkStores<ApplicationDbContext>();

			services.AddControllersWithViews();

			services.AddAuthentication();
			services.AddAuthorization();

			services.AddSwaggerGen(options =>
			{
				options.SwaggerDoc(
					"v1",
					new OpenApiInfo
					{
						Title = "Finance Tracker API",
						Version = "v1",
						Description = "Personal finance tracking API",
						Contact = new OpenApiContact
						{
							Name = "Valters Melnalksnis",
							Email = "valters.melnalksnis@outlook.com",
						},
						License = new OpenApiLicense
						{
							Name = "AGPL-3.0-or-later",
							Url = new Uri("https://www.gnu.org/licenses/agpl-3.0.txt"),
						},
					});

				var xmlDocumentationFilepath = Path.Combine(AppContext.BaseDirectory, "Tracking.Finance.Web.xml");
				options.IncludeXmlComments(xmlDocumentationFilepath, true);
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
				application.UseMigrationsEndPoint();
			}
			else
			{
				application.UseExceptionHandler("/Home/Error");

				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				application.UseHsts();
			}

			application.UseSwagger();
			application.UseSwaggerUI(options =>
			{
				options.SwaggerEndpoint("/swagger/v1/swagger.json", "Finance Tracker API v1");
			});

			application.Use(async (context, next) =>
			{
				context.Response.Headers.Add("Permissions-Policy", "interest-cohort=()");
				await next.Invoke();
			});

			application.UseHttpsRedirection();
			application.UseStaticFiles();
			application.UseCookiePolicy();

			application.UseRouting();
			application.UseRequestLocalization();

			application.UseAuthentication();
			application.UseAuthorization();

			application.UseEndpoints(endpoints =>
			{
				endpoints.MapControllerRoute(
					name: "default",
					pattern: "{controller=Home}/{action=Index}/{id?}");

				endpoints.MapRazorPages();
			});
		}
	}
}
