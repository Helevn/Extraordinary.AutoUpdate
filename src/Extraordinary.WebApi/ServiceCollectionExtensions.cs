using Extraordinary.WebApi.Document;

namespace Extraordinary.WebApi
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
            services.AddSingleton<IFileService, FileService>();
            return services;
        }
    }
}
