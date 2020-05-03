using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace Solaris.Service.Exploration.Presentation
{
    public class Program
    {
        private const string LOG_CONFIG = "NLog.config";

        public static async Task Main(string[] args)
        {
            await Task.Factory.StartNew(() => { CreateWebHostBuilder(args).Build().Run(); });
        }

        private static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureLogging((logging) =>
                {
                    logging.ClearProviders();
                    NLogBuilder.ConfigureNLog(LOG_CONFIG);
                })
                .ConfigureKestrel((context, options) => { options.AllowSynchronousIO = true; })
                .UseWebRoot("")
                .UseNLog()
                .UseStartup<Startup>();
    }
}