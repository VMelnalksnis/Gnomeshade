using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

using Tracking.Finance.Web;

WebHost.CreateDefaultBuilder<Startup>(args).Build().Run();
