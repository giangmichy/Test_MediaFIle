using HD.Station.MediaManagement.Abstractions.Abstractions;
using HD.Station.MediaManagement.Abstractions.Data;
using HD.Station.MediaManagement.Mvc.Features.MediaFile.Models;
using HD.Station.MediaManagement.Mvc.Mapping;
using HD.Station.MediaManagement.Mvc.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using SystemFile = System.IO.File;

namespace HD.Station.MediaManagement.Mvc.Controllers
{
    public class MediaFilesController : Controller
    {
        private readonly IMediaFileManager _manager;
        private readonly IFileProcessor _processor;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<MediaFilesController> _logger;

        public MediaFilesController(
            IMediaFileManager manager,
            IFileProcessor processor,
            IWebHostEnvironment env,
            ILogger<MediaFilesController> logger)
        {
            _manager = manager;
            _processor = processor;
            _env = env;
            _logger = logger;
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
                _logger.LogError(ex, "Error loading media files list");
                TempData["ErrorMessage"] = $"Lỗi tải danh sách file: {ex.Message}";
                return View(Enumerable.Empty<MediaFileViewModel>());
            }
        }

        // GET: /MediaFiles/GetDetails/{id} - API lấy thông tin chi tiết cho JavaScript
        [HttpGet]
        public async Task<IActionResult> GetDetails(Guid id)
        {
            try
            {
                _logger.LogInformation($"Getting details for file ID: {id}");

                var dto = await _manager.GetAsync(id);
                if (dto == null)
                {
                    _logger.LogWarning($"File not found with ID: {id}");
                    return Json(new { success = false, message = "Không tìm thấy file" });
                }

                // Đảm bảo đường dẫn có thể access được từ web
                var storagePath = dto.StoragePath;
                if (!storagePath.StartsWith("/"))
                {
                    storagePath = "/" + storagePath;
                }

                // Kiểm tra file có tồn tại trên disk không
                var physicalPath = Path.Combine(_env.WebRootPath, dto.StoragePath.TrimStart('/'));
                var fileExists = SystemFile.Exists(physicalPath);

                _logger.LogInformation($"File details: {dto.FileName}, Path: {storagePath}, Physical: {physicalPath}, Exists: {fileExists}");

                return Json(new
                {
                    success = true,
                    id = dto.Id,
                    fileName = dto.FileName,
                    description = dto.Description,
                    mediaType = dto.MediaType.ToString(),
                    format = dto.Format.ToString(),
                    size = dto.Size,
                    status = dto.Status.ToString(),
                    storagePath = storagePath,
                    hash = dto.Hash,
                    uploadTime = dto.UploadTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    mediaInfoJson = dto.MediaInfoJson,
                    fileExists = fileExists
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting details for file {id}");
                return Json(new { success = false, message = $"Lỗi tải thông tin file: {ex.Message}" });
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

                _logger.LogInformation($"Starting file upload: {vm.FileUpload.FileName}");

                // 1. Lưu file gốc vào wwwroot/uploads
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsFolder);

                var origName = Path.GetFileName(vm.FileUpload.FileName);
                var safeFileName = Path.GetFileNameWithoutExtension(origName).Replace(" ", "_") + Path.GetExtension(origName);
                var storedName = $"{Guid.NewGuid()}_{safeFileName}";
                var origPath = Path.Combine(uploadsFolder, storedName);

                using (var fs = new FileStream(origPath, FileMode.Create))
                    await vm.FileUpload.CopyToAsync(fs);

                _logger.LogInformation($"File uploaded to: {origPath}");

                // 2. Tính hash MD5
                string hash;
                using (var md5 = MD5.Create())
                using (var stream = SystemFile.OpenRead(origPath))
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
                    _logger.LogWarning(ex, $"FFprobe failed for file: {origPath}");
                }

                // 4. Convert video nếu cần
                var ext = Path.GetExtension(origPath).TrimStart('.').ToLowerInvariant();
                string finalPhysPath = origPath;
                string finalRelPath = $"uploads/{storedName}";

                if (vm.MediaType == MediaTypeEnum.Video && ext != "mp4")
                {
                    try
                    {
                        var convFolder = Path.Combine(_env.WebRootPath, "converted");
                        Directory.CreateDirectory(convFolder);
                        var convPath = await _processor.ConvertToMp4Async(origPath, convFolder);
                        var convName = Path.GetFileName(convPath);
                        finalPhysPath = convPath;
                        finalRelPath = $"converted/{convName}";

                        // Xóa file gốc sau khi convert thành công
                        if (SystemFile.Exists(origPath))
                        {
                            SystemFile.Delete(origPath);
                            _logger.LogInformation($"Original file deleted after conversion: {origPath}");
                        }

                        _logger.LogInformation($"Video converted successfully: {finalPhysPath}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Video conversion failed for: {origPath}");
                        TempData["ErrorMessage"] = $"Lỗi convert video: {ex.Message}";

                        // Clean up original file on conversion failure
                        if (SystemFile.Exists(origPath))
                            SystemFile.Delete(origPath);

                        return RedirectToAction("Index");
                    }
                }

                // 5. Parse extension thành FormatEnum
                if (!Enum.TryParse<FormatEnum>(ext, ignoreCase: true, out var fmt))
                {
                    // Nếu convert thành mp4, set format là MP4
                    fmt = (vm.MediaType == MediaTypeEnum.Video && ext != "mp4") ? FormatEnum.Mp4 : FormatEnum.Other;
                }

                // 6. Verify file exists after processing
                if (!SystemFile.Exists(finalPhysPath))
                {
                    throw new FileNotFoundException($"Processed file not found: {finalPhysPath}");
                }

                // 7. Tạo DTO và lưu vào database
                var dto = new MediaFileDto
                {
                    Id = Guid.NewGuid(),
                    FileName = origName,
                    MediaType = vm.MediaType,
                    Format = fmt,
                    Size = new FileInfo(finalPhysPath).Length,
                    UploadTime = DateTime.UtcNow,
                    StoragePath = finalRelPath, // Không có leading slash
                    Description = vm.Description ?? "",
                    Status = StatusEnum.Active,
                    MediaInfoJson = infoJson,
                    Hash = hash
                };

                var newId = await _manager.CreateAsync(vm.FileUpload, dto);

                _logger.LogInformation($"File created successfully: {dto.FileName} -> {dto.StoragePath}");
                TempData["SuccessMessage"] = $"Upload file thành công! File: {dto.FileName}";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating media file");
                TempData["ErrorMessage"] = $"Lỗi upload file: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // POST: /MediaFiles/Update - Cập nhật file (cho chức năng Edit)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(MediaFileViewModel vm)
        {
            try
            {
                _logger.LogInformation($"Updating file: {vm.Id}");

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    _logger.LogWarning($"Model validation failed: {string.Join(", ", errors)}");
                    TempData["ErrorMessage"] = "Dữ liệu không hợp lệ";
                    return RedirectToAction("Index");
                }

                var existing = await _manager.GetAsync(vm.Id);
                if (existing == null)
                {
                    _logger.LogWarning($"File not found for update: {vm.Id}");
                    TempData["ErrorMessage"] = "Không tìm thấy file";
                    return RedirectToAction("Index");
                }

                // Chỉ update metadata, giữ nguyên file + hash + infoJson
                var dto = new MediaFileDto
                {
                    Id = vm.Id,
                    FileName = existing.FileName, // Giữ nguyên tên file gốc
                    MediaType = vm.MediaType,
                    Format = existing.Format, // Giữ nguyên format
                    Size = existing.Size,
                    UploadTime = existing.UploadTime,
                    StoragePath = existing.StoragePath,
                    Description = vm.Description ?? "",
                    Status = vm.Status,
                    MediaInfoJson = existing.MediaInfoJson,
                    Hash = existing.Hash
                };

                await _manager.UpdateAsync(dto);
                _logger.LogInformation($"File updated successfully: {dto.FileName}");
                TempData["SuccessMessage"] = "Cập nhật file thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating file {vm.Id}");
                TempData["ErrorMessage"] = $"Lỗi cập nhật file: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // POST: /MediaFiles/Delete/{id} - Xóa file (cho chức năng Delete)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                _logger.LogInformation($"Deleting file: {id}");

                // Lấy thông tin file trước khi xóa để xóa file vật lý
                var fileDto = await _manager.GetAsync(id);
                if (fileDto != null && !string.IsNullOrEmpty(fileDto.StoragePath))
                {
                    var filePath = Path.Combine(_env.WebRootPath, fileDto.StoragePath.TrimStart('/'));
                    if (SystemFile.Exists(filePath))
                    {
                        SystemFile.Delete(filePath);
                        _logger.LogInformation($"Physical file deleted: {filePath}");
                    }
                    else
                    {
                        _logger.LogWarning($"Physical file not found for deletion: {filePath}");
                    }
                }

                await _manager.DeleteAsync(id);
                _logger.LogInformation($"File deleted from database: {id}");
                TempData["SuccessMessage"] = "Xóa file thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting file {id}");
                TempData["ErrorMessage"] = $"Lỗi xóa file: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // GET: /MediaFiles/StreamVideo/{id} - Stream video file với range support
        [HttpGet]
        public async Task<IActionResult> StreamVideo(Guid id)
        {
            try
            {
                var fileDto = await _manager.GetAsync(id);
                if (fileDto == null)
                {
                    _logger.LogWarning($"Video file not found in database: {id}");
                    return NotFound();
                }

                var filePath = Path.Combine(_env.WebRootPath, fileDto.StoragePath.TrimStart('/'));
                if (!SystemFile.Exists(filePath))
                {
                    _logger.LogWarning($"Video file not found on disk: {filePath}");
                    return NotFound();
                }

                var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var contentType = GetContentType(fileDto.Format);

                return File(fileStream, contentType, enableRangeProcessing: true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error streaming video {id}");
                return StatusCode(500);
            }
        }

        // Helper method để get content type
        private string GetContentType(FormatEnum format)
        {
            return format switch
            {
                FormatEnum.Mp4 => "video/mp4",
                FormatEnum.Avi => "video/avi",
                FormatEnum.Mov => "video/quicktime",
                FormatEnum.Wmv => "video/x-ms-wmv",
                FormatEnum.Mp3 => "audio/mpeg",
                FormatEnum.Wav => "audio/wav",
                FormatEnum.Ogg => "audio/ogg",
                FormatEnum.Flac => "audio/flac",
                FormatEnum.Jpg => "image/jpeg",
                FormatEnum.Png => "image/png",
                FormatEnum.Gif => "image/gif",
                FormatEnum.Bmp => "image/bmp",
                FormatEnum.Svg => "image/svg+xml",
                FormatEnum.Pdf => "application/pdf",
                FormatEnum.Doc => "application/msword",
                FormatEnum.Docx => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                FormatEnum.Txt => "text/plain",
                _ => "application/octet-stream"
            };
        }
    }
}