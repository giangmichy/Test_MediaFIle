using System.IO;
using Microsoft.Extensions.FileProviders;
using HD.Station.MediaManagement.Abstractions.DependencyInjection;
using HD.Station.MediaManagement.SqlServer.DependencyInjection;
using HD.Station.MediaManagement.Mvc.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// 1. Đăng ký toàn bộ Feature, Store, MVC (controllers + views)
builder.Services
    .AddMediaManagementFeature()
    .AddMediaManagementSqlServer(builder.Configuration)
    .AddMediaManagementMvc();    // đã bao gồm AddControllersWithViews & Razor runtime

// 2. Cấu hình serve MediaShare folder
var mediaPath = builder.Configuration["MediaSharePath"];
if (!Directory.Exists(mediaPath))
    Directory.CreateDirectory(mediaPath);

builder.Services.AddSingleton<IFileProvider>(_ =>
    new PhysicalFileProvider(mediaPath));

var app = builder.Build();

// 3. Middleware
app.UseHttpsRedirection();
app.UseStaticFiles();  // wwwroot
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(mediaPath),
    RequestPath = "/media"
});
app.UseRouting();
app.UseAuthorization();

// 4. Route mặc định đến MediaFilesController.Index
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=MediaFiles}/{action=Index}/{id?}"
);

app.Run();
