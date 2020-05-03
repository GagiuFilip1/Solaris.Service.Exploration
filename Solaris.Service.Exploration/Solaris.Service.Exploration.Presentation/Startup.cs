using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Solaris.Service.Exploration.Core.Models.Helpers.Commons;
using Solaris.Service.Exploration.Infrastructure.Ioc;
using Solaris.Service.Exploration.Presentation.Handlers.implementation;

namespace Solaris.Service.Exploration.Presentation
{
    public class Startup
    {
        private const string SERVICES_NAMESPACE = "Solaris.Service.Exploration.Infrastructure.Services";
        private const string HANDLERS_NAMESPACE = "Solaris.Service.Exploration.Presentation.Handlers.implementation";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<KestrelServerOptions>(options => { options.AllowSynchronousIO = true; });
            services.Configure<IISServerOptions>(options => { options.AllowSynchronousIO = true; });
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
            services.InjectForNamespace(SERVICES_NAMESPACE);
            services.InjectForNamespace(HANDLERS_NAMESPACE);
            services.InjectRabbitMq();
            
            var manager = services.BuildServiceProvider().GetRequiredService<HandlersManager>();
            manager.HandleRequests();
        }

        public void Configure(IApplicationBuilder app)
        {
            
        }
    }
}