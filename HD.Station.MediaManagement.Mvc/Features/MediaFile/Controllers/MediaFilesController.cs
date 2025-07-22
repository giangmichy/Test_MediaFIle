using HD.Station.MediaManagement.Abstractions.Abstractions;
using HD.Station.MediaManagement.Abstractions.Data;
using HD.Station.MediaManagement.Mvc.Features.MediaFile.Models;
using HD.Station.MediaManagement.Mvc.Mapping;
using HD.Station.MediaManagement.Mvc.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using SystemFile = System.IO.File; // Alias để tránh xung đột

namespace HD.Station.MediaManagement.Mvc.Controllers
{
    public class MediaFilesController : Controller
    {
        private readonly IMediaFileManager _manager;
        private readonly IFileProcessor _processor;
        private readonly IWebHostEnvironment _env;

        public MediaFilesController(
            IMediaFileManager manager,
            IFileProcessor processor,
            IWebHostEnvironment env)
        {
            _manager = manager;
            _processor = processor;
            _env = env;
        }

        // GET: /MediaFiles
        public async Task<IActionResult> Index(string filter = "", int page = 1)
        {
            try
            {
                const int pageSize = 12;
                var dtos = await _manager.ListAsync(filter, page, pageSize);
                var vms = dtos.Select(d => d.ToViewModel());
                ViewBag.Filter = filter;
                ViewBag.Page = page;
                return View(vms);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi tải danh sách file: {ex.Message}";
                return View(Enumerable.Empty<MediaFileViewModel>());
            }
        }

        // GET: /MediaFiles/GetDetails/{id} - API lấy thông tin chi tiết
        [HttpGet]
        public async Task<IActionResult> GetDetails(Guid id)
        {
            try
            {
                var dto = await _manager.GetAsync(id);
                if (dto == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy file" });
                }

                var vm = dto.ToViewModel();
                return Json(new
                {
                    success = true,
                    id = vm.Id,
                    fileName = vm.FileName,
                    description = vm.Description,
                    mediaType = vm.MediaType.ToString(),
                    format = vm.Format.ToString(),
                    size = vm.Size,
                    status = vm.Status.ToString(),
                    storagePath = vm.StoragePath,
                    hash = vm.Hash,
                    uploadTime = vm.UploadTime,
                    mediaInfoJson = vm.MediaInfoJson
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: /MediaFiles/Create - Tạo file mới với FFmpeg
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MediaFileViewModel vm)
        {
            try
            {
                if (vm.FileUpload == null || vm.FileUpload.Length == 0)
                {
                    TempData["ErrorMessage"] = "Vui lòng chọn file để upload";
                    return RedirectToAction("Index");
                }

                // 1. Lưu file gốc vào wwwroot/uploads
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsFolder);

                var origName = Path.GetFileName(vm.FileUpload.FileName);
                var storedName = $"{Guid.NewGuid()}_{origName}";
                var origPath = Path.Combine(uploadsFolder, storedName);

                using (var fs = new FileStream(origPath, FileMode.Create))
                    await vm.FileUpload.CopyToAsync(fs);

                // 2. Tính hash MD5
                string hash;
                using (var md5 = MD5.Create())
                using (var stream = SystemFile.OpenRead(origPath)) // Sử dụng alias
                {
                    hash = BitConverter.ToString(md5.ComputeHash(stream))
                                     .Replace("-", "")
                                     .ToLowerInvariant();
                }

                // 3. Lấy metadata JSON bằng FFprobe
                string infoJson = "{}";
                try
                {
                    infoJson = await _processor.GetMediaInfoJsonAsync(origPath);
                }
                catch (Exception ex)
                {
                    // Log lỗi nhưng không dừng quá trình upload
                    Console.WriteLine($"FFprobe error: {ex.Message}");
                }

                // 4. Convert video nếu cần
                var ext = Path.GetExtension(origPath).TrimStart('.').ToLowerInvariant();
                string finalPhysPath = origPath;
                string finalRelPath = $"/uploads/{storedName}";

                if (vm.MediaType == MediaTypeEnum.Video && ext != "mp4")
                {
                    try
                    {
                        var convFolder = Path.Combine(_env.WebRootPath, "converted");
                        Directory.CreateDirectory(convFolder);
                        var convPath = await _processor.ConvertToMp4Async(origPath, convFolder);
                        var convName = Path.GetFileName(convPath);
                        finalPhysPath = convPath;
                        finalRelPath = $"/converted/{convName}";

                        // Xóa file gốc sau khi convert thành công
                        if (SystemFile.Exists(origPath)) // Sử dụng alias
                            SystemFile.Delete(origPath); // Sử dụng alias
                    }
                    catch (Exception ex)
                    {
                        TempData["ErrorMessage"] = $"Lỗi convert video: {ex.Message}";
                        return RedirectToAction("Index");
                    }
                }

                // 5. Parse extension thành FormatEnum
                if (!Enum.TryParse<FormatEnum>(ext, ignoreCase: true, out var fmt))
                {
                    // Nếu convert thành mp4, set format là MP4
                    fmt = (vm.MediaType == MediaTypeEnum.Video && ext != "mp4") ? FormatEnum.Mp4 : FormatEnum.Other;
                }

                // 6. Tạo DTO và lưu vào database
                var dto = new MediaFileDto
                { 
                    Id = Guid.NewGuid(),
                    FileName = origName,
                    MediaType = vm.MediaType,
                    Format = fmt,
                    Size = new FileInfo(finalPhysPath).Length,
                    UploadTime = DateTime.UtcNow,
                    StoragePath = finalRelPath,
                    Description = vm.Description ?? "",
                    Status = StatusEnum.Active,
                    MediaInfoJson = infoJson,
                    Hash = hash
                };

                var newId = await _manager.CreateAsync(vm.FileUpload, dto);

                TempData["SuccessMessage"] = $"Upload file thành công! Đường dẫn: {finalRelPath}";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi upload file: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // POST: /MediaFiles/Update - Cập nhật file
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(MediaFileViewModel vm)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "Dữ liệu không hợp lệ";
                    return RedirectToAction("Index");
                }

                var existing = await _manager.GetAsync(vm.Id);
                if (existing == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy file";
                    return RedirectToAction("Index");
                }

                // Chỉ update metadata, giữ nguyên file + hash + infoJson
                var dto = new MediaFileDto
                {
                    Id = vm.Id,
                    FileName = existing.FileName,
                    MediaType = vm.MediaType,
                    Format = vm.Format,
                    Size = existing.Size,
                    UploadTime = existing.UploadTime,
                    StoragePath = existing.StoragePath,
                    Description = vm.Description,
                    Status = vm.Status,
                    MediaInfoJson = existing.MediaInfoJson,
                    Hash = existing.Hash
                };

                await _manager.UpdateAsync(dto);
                TempData["SuccessMessage"] = "Cập nhật file thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi cập nhật file: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // POST: /MediaFiles/Delete/{id} - Xóa file
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                // Lấy thông tin file trước khi xóa để xóa file vật lý
                var fileDto = await _manager.GetAsync(id);
                if (fileDto != null && !string.IsNullOrEmpty(fileDto.StoragePath))
                {
                    var filePath = Path.Combine(_env.WebRootPath, fileDto.StoragePath.TrimStart('/'));
                    if (SystemFile.Exists(filePath)) // Sử dụng alias
                    {
                        SystemFile.Delete(filePath); // Sử dụng alias
                    }
                }

                await _manager.DeleteAsync(id);
                TempData["SuccessMessage"] = "Xóa file thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi xóa file: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
    }
}