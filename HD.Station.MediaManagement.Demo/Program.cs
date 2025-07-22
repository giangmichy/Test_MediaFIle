using HD.Station.MediaManagement.Abstractions.DependencyInjection;
using HD.Station.MediaManagement.SqlServer.DependencyInjection;
using HD.Station.MediaManagement.Mvc.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// 1. Đăng ký toàn bộ Feature, Store, MVC
builder.Services
    .AddMediaManagementFeature()
    .AddMediaManagementSqlServer(builder.Configuration)
    .AddMediaManagementMvc();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Static files - quan trọng!
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

// 2. Route mặc định đến MediaFilesController.Index
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=MediaFiles}/{action=Index}/{id?}");

// 3. Tạo thư mục uploads và converted nếu chưa có
var uploadsDir = Path.Combine(app.Environment.WebRootPath, "uploads");
var convertedDir = Path.Combine(app.Environment.WebRootPath, "converted");

if (!Directory.Exists(uploadsDir))
{
    Directory.CreateDirectory(uploadsDir);
}

if (!Directory.Exists(convertedDir))
{
    Directory.CreateDirectory(convertedDir);
}

app.Run();