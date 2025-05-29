using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Razor;
using HD.Station.MediaManagement.Mvc.Features.MediaFile.Controllers;

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
                    // Format: /Features/MediaFile/Views/{ControllerName}/{ViewName}.cshtml
                    opts.ViewLocationFormats.Insert(0,
                        "/Features/MediaFile/Views/{1}/{0}.cshtml");
                });

            return services;
        }
    }
}


//using Microsoft.Extensions.DependencyInjection;

//namespace HD.Station.MediaManagement.Mvc.DependencyInjection
//{
//    public static class DependencyInjectionExtensions
//    {
//        public static IServiceCollection AddMediaManagementMvc(this IServiceCollection services)
//        {
//            services.AddControllersWithViews()
//                    .AddRazorRuntimeCompilation()
//                    ;

//            return services;
//        }
//    }
//}
