using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Tracking.Finance.Web;
using Tracking.Finance.Web.Data;

var host = WebHost.CreateDefaultBuilder<Startup>(args).Build();
//using (var scope = host.Services.CreateScope())
//{
//	await scope.ServiceProvider.SeedDatabase();
//}

host.Run();
