using Extraordinary.App.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Reflection;

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
                    _ => throw new Exception($"未知的URL={url}"),
                };
                return vm;
            });

            services.AddSingleton<MainWindow>();
            services.AddSingleton<Realtime>();
            services.AddSingleton<RealtimeViewModel>();

            return services;
        }
    }
}
