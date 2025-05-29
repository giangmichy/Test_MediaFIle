
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using HD.Station.MediaManagement.Abstractions.Stores;
using HD.Station.MediaManagement.SqlServer.DbContexts;
using HD.Station.MediaManagement.SqlServer.Stores;

namespace HD.Station.MediaManagement.SqlServer.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        /// <summary>
        /// Đăng ký DbContext và MediaFileStore
        /// </summary>
        public static IServiceCollection AddMediaManagementSqlServer(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Đọc connection string từ appsettings.json
            var conn = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<MediaManagementDbContext>(options =>
                options.UseSqlServer(conn));

            // Đăng ký store
            services.AddScoped<IMediaFileStore, MediaFileStore>();

            return services;
        }
    }
}
