using HD.Station.MediaManagement.Mvc.Controllers;
using HD.Station.MediaManagement.Mvc.Services;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;

namespace HD.Station.MediaManagement.Mvc.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddMediaManagementMvc(this IServiceCollection services)
        {
            services
                .AddControllersWithViews()
                    .AddRazorRuntimeCompilation()
                    // Đăng ký assembly chứa các MediaFilesController
                    .AddApplicationPart(typeof(MediaFilesController).Assembly)
                .Services

                // Tùy chỉnh nơi tìm view theo Feature-Folder
                .Configure<RazorViewEngineOptions>(opts =>
                {
                    opts.ViewLocationFormats.Clear();
                    opts.ViewLocationFormats.Add("/Features/MediaFile/Views/{0}.cshtml");
                });

            // Đăng ký các services
            services.AddSingleton<IFileProcessor, FileProcessor>();
            services.AddScoped<IFileStorageService, FileStorageService>();

            return services;
        }
    }
}