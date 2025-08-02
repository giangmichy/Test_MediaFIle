using HD.Station.MediaManagement.Abstractions.Abstractions;
using HD.Station.MediaManagement.Abstractions.Data;
using HD.Station.MediaManagement.Mvc.Features.MediaFile.Models;
using HD.Station.MediaManagement.Mvc.Mapping;
using HD.Station.MediaManagement.Mvc.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
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
        private readonly IFileStorageService _storageService;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<MediaFilesController> _logger;
        private readonly IConfiguration _config;

        // File size limits (5GB)
        private const long MAX_FILE_SIZE = 5L * 1024 * 1024 * 1024;

        public MediaFilesController(
            IMediaFileManager manager,
            IFileProcessor processor,
            IFileStorageService storageService,
            IWebHostEnvironment env,
            ILogger<MediaFilesController> logger,
            IConfiguration config)
        {
            _manager = manager;
            _processor = processor;
            _storageService = storageService;
            _env = env;
            _logger = logger;
            _config = config;
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

                // Xử lý đường dẫn theo storage type
                string accessPath = "";
                bool fileExists = false;

                switch (dto.StorageType)
                {
                    case StorageTypeEnum.Local:
                        accessPath = dto.StoragePath.StartsWith("/") ? dto.StoragePath : "/" + dto.StoragePath;
                        var physicalPath = Path.Combine(_env.WebRootPath, dto.StoragePath.TrimStart('/'));
                        fileExists = SystemFile.Exists(physicalPath);
                        break;

                    case StorageTypeEnum.UNC:
                    case StorageTypeEnum.FTP:
                        accessPath = dto.NetworkPath ?? dto.StoragePath;
                        fileExists = await _storageService.FileExistsAsync(dto.StoragePath, dto.StorageType);
                        break;
                }

                _logger.LogInformation($"File details: {dto.FileName}, Storage: {dto.StorageType}, Path: {accessPath}, Exists: {fileExists}");

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
                    storageType = dto.StorageType.ToString(),
                    storagePath = accessPath,
                    networkPath = dto.NetworkPath,
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

        // POST: /MediaFiles/Create - Tạo file mới với multiple storage options
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(5L * 1024 * 1024 * 1024)] // 5GB limit
        [RequestFormLimits(MultipartBodyLengthLimit = 5L * 1024 * 1024 * 1024)]
        public async Task<IActionResult> Create(MediaFileViewModel vm)
        {
            try
            {
                if (vm.FileUpload == null || vm.FileUpload.Length == 0)
                {
                    TempData["ErrorMessage"] = "Vui lòng chọn file để upload";
                    return RedirectToAction("Index");
                }

                if (vm.FileUpload.Length > MAX_FILE_SIZE)
                {
                    TempData["ErrorMessage"] = $"File quá lớn! Kích thước tối đa là {MAX_FILE_SIZE / (1024 * 1024 * 1024)}GB";
                    return RedirectToAction("Index");
                }

                _logger.LogInformation($"Starting file upload: {vm.FileUpload.FileName} ({vm.FileUpload.Length:N0} bytes) to {vm.StorageType}");

                // 1. Upload file theo storage type
                string storagePath;
                using (var stream = vm.FileUpload.OpenReadStream())
                {
                    storagePath = await _storageService.UploadFileAsync(stream, vm.FileUpload.FileName, vm.StorageType);
                }

                _logger.LogInformation($"File uploaded to {vm.StorageType}: {storagePath}");

                // 2. Tính hash MD5
                string hash;
                using (var md5 = MD5.Create())
                using (var stream = vm.FileUpload.OpenReadStream())
                {
                    hash = BitConverter.ToString(await md5.ComputeHashAsync(stream))
                                     .Replace("-", "")
                                     .ToLowerInvariant();
                }

                // 3. Lấy metadata JSON bằng FFprobe cho mọi storage
                string infoJson = "{}";
                try
                {
                    string probePath;

                    if (vm.StorageType == StorageTypeEnum.Local)
                    {
                        // Sử dụng _env.WebRootPath để có đường dẫn tuyệt đối
                        probePath = Path.Combine(_env.WebRootPath, storagePath.TrimStart('/').Replace("/", "\\"));
                    }
                    else
                    {
                        var tempFolder = Path.Combine(_env.WebRootPath, "temp");
                        if (!Directory.Exists(tempFolder))
                        {
                            Directory.CreateDirectory(tempFolder);
                        }

                        var tempFile = Path.Combine(tempFolder, Path.GetFileName(vm.FileUpload.FileName));
                        using (var tempStream = new FileStream(tempFile, FileMode.Create, FileAccess.Write))
                        {
                            await vm.FileUpload.CopyToAsync(tempStream);
                        }

                        probePath = tempFile;
                    }

                    infoJson = await _processor.GetMediaInfoJsonAsync(probePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"FFprobe failed for file: {vm.FileUpload.FileName}");
                }


                // 4. Parse extension thành FormatEnum
                var ext = Path.GetExtension(vm.FileUpload.FileName).TrimStart('.').ToLowerInvariant();
                FormatEnum fmt = ext switch
                {
                    "jpg" => FormatEnum.Jpg,
                    "jpeg" => FormatEnum.Jpeg,
                    "png" => FormatEnum.Png,
                    "gif" => FormatEnum.Gif,
                    "bmp" => FormatEnum.Bmp,
                    "svg" => FormatEnum.Svg,
                    "webp" => FormatEnum.Webp,
                    "tiff" or "tif" => FormatEnum.Tiff,
                    "mp4" => FormatEnum.Mp4,
                    "avi" => FormatEnum.Avi,
                    "mov" => FormatEnum.Mov,
                    "wmv" => FormatEnum.Wmv,
                    "mkv" => FormatEnum.Mkv,
                    "flv" => FormatEnum.Flv,
                    "webm" => FormatEnum.Webm,
                    "mpeg" or "mpg" => FormatEnum.Mpeg,
                    "mp3" => FormatEnum.Mp3,
                    "wav" => FormatEnum.Wav,
                    "ogg" => FormatEnum.Ogg,
                    "flac" => FormatEnum.Flac,
                    "aac" => FormatEnum.Aac,
                    "m4a" => FormatEnum.M4a,
                    "wma" => FormatEnum.Wma,
                    "pdf" => FormatEnum.Pdf,
                    "doc" => FormatEnum.Doc,
                    "docx" => FormatEnum.Docx,
                    "xls" => FormatEnum.Xls,
                    "xlsx" => FormatEnum.Xlsx,
                    "ppt" => FormatEnum.Ppt,
                    "pptx" => FormatEnum.Pptx,
                    "txt" => FormatEnum.Txt,
                    _ => FormatEnum.Other
                };

                // 5. Lưu DTO vào DB
                var dto = new MediaFileDto
                {
                    Id = Guid.NewGuid(),
                    FileName = vm.FileUpload.FileName,
                    MediaType = vm.MediaType,
                    Format = fmt,
                    Size = vm.FileUpload.Length,
                    UploadTime = DateTime.UtcNow,
                    StoragePath = storagePath,
                    Description = vm.Description ?? "",
                    Status = StatusEnum.Active,
                    MediaInfoJson = infoJson,
                    Hash = hash,
                    StorageType = vm.StorageType,
                    NetworkPath = vm.StorageType != StorageTypeEnum.Local ? _storageService.GetFullPath(storagePath, vm.StorageType) : null
                };

                var newId = await _manager.CreateAsync(vm.FileUpload, dto);

                _logger.LogInformation($"File created successfully: {dto.FileName} -> {vm.StorageType}:{dto.StoragePath} ({dto.Size:N0} bytes)");
                TempData["SuccessMessage"] = $"Upload file thành công! File: {dto.FileName} - Lưu trữ: {vm.StorageType} ({(dto.Size / (1024.0 * 1024)):F1} MB)";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating media file with storage service");
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
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
                }

                var existing = await _manager.GetAsync(vm.Id);
                if (existing == null)
                {
                    _logger.LogWarning($"File not found for update: {vm.Id}");
                    return Json(new { success = false, message = "Không tìm thấy file" });
                }

                // Chỉ update metadata, giữ nguyên file info
                var dto = new MediaFileDto
                {
                    Id = vm.Id,
                    FileName = existing.FileName,
                    MediaType = vm.MediaType,
                    Format = existing.Format,
                    Size = existing.Size,
                    UploadTime = existing.UploadTime,
                    StoragePath = existing.StoragePath,
                    Description = vm.Description ?? "",
                    Status = vm.Status,
                    MediaInfoJson = existing.MediaInfoJson,
                    Hash = existing.Hash,
                    StorageType = existing.StorageType, // Không cho phép thay đổi storage type
                    NetworkPath = existing.NetworkPath
                };

                await _manager.UpdateAsync(dto);
                _logger.LogInformation($"File updated successfully: {dto.FileName}");
                return Json(new { success = true, message = "Cập nhật file thành công!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating file {vm.Id}");
                return Json(new { success = false, message = $"Lỗi cập nhật file: {ex.Message}" });
            }
        }

        // POST: /MediaFiles/Delete/{id} - Xóa file với storage service
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                _logger.LogInformation($"Deleting file: {id}");

                // Lấy thông tin file trước khi xóa
                var fileDto = await _manager.GetAsync(id);
                if (fileDto != null && !string.IsNullOrEmpty(fileDto.StoragePath))
                {
                    // Xóa file vật lý theo storage type
                    var deleted = await _storageService.DeleteFileAsync(fileDto.StoragePath, fileDto.StorageType);

                    if (deleted)
                    {
                        _logger.LogInformation($"Physical file deleted from {fileDto.StorageType}: {fileDto.StoragePath}");
                    }
                    else
                    {
                        _logger.LogWarning($"Could not delete physical file from {fileDto.StorageType}: {fileDto.StoragePath}");
                    }
                }

                await _manager.DeleteAsync(id);
                _logger.LogInformation($"File deleted from database: {id}");
                return Json(new { success = true, message = "Xóa file thành công!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting file {id}");
                return Json(new { success = false, message = $"Lỗi xóa file: {ex.Message}" });
            }
        }

        // GET: /MediaFiles/Download/{id} - Download file từ bất kỳ storage nào
        [HttpGet]
        public async Task<IActionResult> Download(Guid id)
        {
            try
            {
                var fileDto = await _manager.GetAsync(id);
                if (fileDto == null)
                {
                    return NotFound("File không tồn tại");
                }

                // Chỉ hỗ trợ download cho Local storage
                if (fileDto.StorageType == StorageTypeEnum.Local)
                {
                    var filePath = Path.Combine(_env.WebRootPath, fileDto.StoragePath.TrimStart('/'));
                    if (!SystemFile.Exists(filePath))
                    {
                        return NotFound("File không tồn tại trên hệ thống");
                    }

                    var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                    var contentType = GetContentType(fileDto.Format);

                    return File(fileStream, contentType, fileDto.FileName);
                }
                else
                {
                    // Đối với UNC và FTP, redirect đến đường dẫn network
                    return Json(new
                    {
                        success = false,
                        message = $"File được lưu trữ ở {fileDto.StorageType}. Đường dẫn: {fileDto.NetworkPath}",
                        downloadUrl = fileDto.NetworkPath
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error downloading file {id}");
                return StatusCode(500, "Lỗi tải file");
            }
        }

        // Helper method để get content type
        private string GetContentType(FormatEnum format)
        {
            return format switch
            {
                // Image formats
                FormatEnum.Jpg => "image/jpeg",
                FormatEnum.Jpeg => "image/jpeg",
                FormatEnum.Png => "image/png",
                FormatEnum.Gif => "image/gif",
                FormatEnum.Bmp => "image/bmp",
                FormatEnum.Svg => "image/svg+xml",
                FormatEnum.Webp => "image/webp",
                FormatEnum.Tiff => "image/tiff",

                // Video formats
                FormatEnum.Mp4 => "video/mp4",
                FormatEnum.Avi => "video/avi",
                FormatEnum.Mov => "video/quicktime",
                FormatEnum.Wmv => "video/x-ms-wmv",
                FormatEnum.Mkv => "video/x-matroska",
                FormatEnum.Flv => "video/x-flv",
                FormatEnum.Webm => "video/webm",
                FormatEnum.Mpeg => "video/mpeg",

                // Audio formats
                FormatEnum.Mp3 => "audio/mpeg",
                FormatEnum.Wav => "audio/wav",
                FormatEnum.Ogg => "audio/ogg",
                FormatEnum.Flac => "audio/flac",
                FormatEnum.Aac => "audio/aac",
                FormatEnum.M4a => "audio/mp4",
                FormatEnum.Wma => "audio/x-ms-wma",

                // Document formats
                FormatEnum.Pdf => "application/pdf",
                FormatEnum.Doc => "application/msword",
                FormatEnum.Docx => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                FormatEnum.Xls => "application/vnd.ms-excel",
                FormatEnum.Xlsx => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                FormatEnum.Ppt => "application/vnd.ms-powerpoint",
                FormatEnum.Pptx => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                FormatEnum.Txt => "text/plain",

                // Default
                _ => "application/octet-stream"
            };
        }
    }
}