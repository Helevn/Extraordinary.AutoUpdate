using Extraordinary.Services.Application;
using Extraordinary.Services.Document;
using Microsoft.Extensions.DependencyInjection;

namespace Extraordinary.Services
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddSingleton<IFileService, FileService>();
            services.AddSingleton<IProcessService, ProcessService>();
            return services;
        }
    }
}
