using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

using Tracking.Finance.Interfaces.WebApi;

WebHost.CreateDefaultBuilder<Startup>(args).Build().Run();
