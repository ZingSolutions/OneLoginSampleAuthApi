using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace OneLoginSampleAuthApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            .ConfigureLogging(logBuilder =>
            {
               logBuilder.ClearProviders();
               logBuilder.AddConsole();
                //TODO: add logging provider for production
                // logBuilder.Add...
            })
            .UseStartup<Startup>();
    }
}
