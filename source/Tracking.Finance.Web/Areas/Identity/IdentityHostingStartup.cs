using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup(typeof(Tracking.Finance.Web.Areas.Identity.IdentityHostingStartup))]

namespace Tracking.Finance.Web.Areas.Identity
{
	public class IdentityHostingStartup : IHostingStartup
	{
		/// <inheritdoc/>
		public void Configure(IWebHostBuilder builder)
		{
			builder.ConfigureServices((context, services) =>
			{
			});
		}
	}
}