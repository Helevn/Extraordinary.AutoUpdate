using Microsoft.Extensions.DependencyInjection;

namespace Extraordinary.App.Services
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddSingleton<IFileServices, FileServices>();
            return services;
        }
    }
}
