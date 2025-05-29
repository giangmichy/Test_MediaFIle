using Microsoft.Extensions.DependencyInjection;
using HD.Station.MediaManagement.Abstractions.Abstractions;
using HD.Station.MediaManagement.Abstractions.Services;
using HD.Station.MediaManagement.Abstractions.Stores;

namespace HD.Station.MediaManagement.Abstractions.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddMediaManagementFeature(this IServiceCollection services)
        {
            services.AddScoped<IMediaFileManager, MediaFileManager>();
            // Store sẽ được override bởi project SqlServer
            /*services.AddScoped<IMediaFileStore, MediaFileStore>();*/
            return services;
        }
    }
}
