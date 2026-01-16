using Extraordinary.App.Views;
using Extraordinary.Services;
using Extraordinary.Shared;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Extraordinary.App
{
    internal static class HostStartup
    {
        public static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(App).Assembly));
            services.AddOptions<BaseConfigOpt>()
                .Bind(context.Configuration.GetSection("ConfigOpt"));

            services.AddViews();
            services.AddServices();
        }
    }
}
