using Extraordinary.App.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Extraordinary.App.Views
{

    public static class ViewsServiceCollectionExtensions
    {
        public static IServiceCollection AddViews(this IServiceCollection services)
        {
            services.AddSingleton(sp =>
            {
                var vm = ActivatorUtilities.CreateInstance<AppViewModel>(sp);
                vm.MapSourceToPage = url => url switch
                {
                    UrlDefines.URL_Realtime => sp.GetRequiredService<Realtime>(),
                    UrlDefines.URL_Params => sp.GetRequiredService<ParamMgmt>(),
                    _ => throw new Exception($"未知的URL={url}"),
                };
                return vm;
            });

            services.AddSingleton<MainWindow>();
            services.AddSingleton<Realtime>();
            services.AddSingleton<RealtimeViewModel>();
            services.AddSingleton<ParamMgmt>();
            services.AddSingleton<ParamMgmtVM>();

            return services;
        }
    }
}
