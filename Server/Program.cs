using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;
using NestorHub.Server.Class;

namespace NestorHub.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            var hostingConfiguration = new HostingConfiguration();

            return WebHost.CreateDefaultBuilder(args)
                .ConfigureLogging(builder =>
                {
                    builder.AddFilter<ApplicationInsightsLoggerProvider>("", LogLevel.Error);
                    builder.AddFilter<ApplicationInsightsLoggerProvider>("Common.Api.Class.HomeControllerLogCategory",
                        LogLevel.Information);
                    builder.AddApplicationInsights("555b5423-78ac-4329-929c-f5f93439f3c7");
                })
                .UseUrls(hostingConfiguration.GetUrlToUse())
                .UseStartup<Startup>();
        }
    }
}
