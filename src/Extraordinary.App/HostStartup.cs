using Extraordinary.App.Opt;
using Extraordinary.App.Services;
using Extraordinary.App.Views;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Extraordinary.App
{
    internal static class HostStartup
    {
        public static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            services.AddMediatR(typeof(App).Assembly);

            services.AddOptions<ConfigOpt>()
                .Bind(context.Configuration.GetSection("ConfigOpt"));

            services.AddViews();
            services.AddServices();
        }
    }
}
