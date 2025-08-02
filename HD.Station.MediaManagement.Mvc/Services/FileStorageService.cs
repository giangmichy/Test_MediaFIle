using HD.Station.MediaManagement.Abstractions.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace HD.Station.MediaManagement.Mvc.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<FileStorageService> _logger;
        private readonly string _localStoragePath;
        private readonly string _uncBasePath;
        private readonly string _ftpServer;
        private readonly string _ftpUsername;
        private readonly string _ftpPassword;

        public FileStorageService(IConfiguration config, ILogger<FileStorageService> logger)
        {
            _config = config;
            _logger = logger;

            // Đọc config paths
            _localStoragePath = _config["FileStorageSettings:LocalPath"] ?? "wwwroot";
            _uncBasePath = _config["FileStorageSettings:UNCPath"] ?? @"\\server\MediaShare";
            _ftpServer = _config["FileStorageSettings:FTPServer"] ?? "ftp://localhost";
            _ftpUsername = _config["FileStorageSettings:FTPUsername"] ?? "ftpuser";
            _ftpPassword = _config["FileStorageSettings:FTPPassword"] ?? "ftppass";
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, StorageTypeEnum storageType)
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyyMMdd");
                var relativePath = "";

                switch (storageType)
                {
                    case StorageTypeEnum.Local:
                        relativePath = await UploadToLocalAsync(fileStream, fileName, timestamp);
                        break;
                    case StorageTypeEnum.UNC:
                        relativePath = await UploadToUNCAsync(fileStream, fileName, timestamp);
                        break;
                    case StorageTypeEnum.FTP:
                        relativePath = await UploadToFTPAsync(fileStream, fileName, timestamp);
                        break;
                    default:
                        throw new ArgumentException($"Unsupported storage type: {storageType}");
                }

                _logger.LogInformation($"File uploaded successfully: {fileName} to {storageType} at {relativePath}");
                return relativePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to upload file {fileName} to {storageType}");
                throw;
            }
        }

        private async Task<string> UploadToLocalAsync(Stream fileStream, string fileName, string timestamp)
        {
            var uploadsFolder = Path.Combine(_localStoragePath, "uploads", timestamp);
            Directory.CreateDirectory(uploadsFolder);

            var safeFileName = GetSafeFileName(fileName);
            var filePath = Path.Combine(uploadsFolder, safeFileName);

            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                await fileStream.CopyToAsync(fs);
                await fs.FlushAsync();
            }

            return Path.Combine("uploads", timestamp, safeFileName).Replace("\\", "/");
        }

        private async Task<string> UploadToUNCAsync(Stream fileStream, string fileName, string timestamp)
        {
            try
            {
                var uncFolder = Path.Combine(_uncBasePath, "UNC_Storage", timestamp);

                // Tạo thư mục nếu chưa có
                if (!Directory.Exists(uncFolder))
                {
                    Directory.CreateDirectory(uncFolder);
                    _logger.LogInformation($"Created UNC directory: {uncFolder}");
                }

                var safeFileName = GetSafeFileName(fileName);
                var filePath = Path.Combine(uncFolder, safeFileName);

                using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    await fileStream.CopyToAsync(fs);
                    await fs.FlushAsync();
                }

                _logger.LogInformation($"File uploaded to UNC: {filePath}");
                return Path.Combine("UNC_Storage", timestamp, safeFileName).Replace("\\", "/");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to upload to UNC path: {_uncBasePath}");
                throw new InvalidOperationException($"UNC upload failed: {ex.Message}", ex);
            }
        }

        private async Task<string> UploadToFTPAsync(Stream fileStream, string fileName, string timestamp)
        {
            try
            {
                var safeFileName = GetSafeFileName(fileName);
                var ftpDirPath = $"{_ftpServer.TrimEnd('/')}/FTP_Storage/{timestamp}";
                var ftpPath = $"{ftpDirPath}/{safeFileName}";

                // Tạo thư mục cha (nếu chưa có)
                await CreateFTPDirectoryAsync($"{_ftpServer.TrimEnd('/')}/FTP_Storage");
                await CreateFTPDirectoryAsync(ftpDirPath);

                var request = (FtpWebRequest)WebRequest.Create(ftpPath);
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.Credentials = new NetworkCredential(_ftpUsername, _ftpPassword);
                request.UseBinary = true;
                request.UsePassive = _config.GetValue<bool>("FileStorageSettings:FTPPassiveMode", true);
                request.KeepAlive = true; // sửa lại ở đây
                request.EnableSsl = false;

                using (var requestStream = await request.GetRequestStreamAsync())
                {
                    await fileStream.CopyToAsync(requestStream);
                }

                using (var response = (FtpWebResponse)await request.GetResponseAsync())
                {
                    _logger.LogInformation($"FTP upload completed: {response.StatusDescription}");
                    return $"FTP_Storage/{timestamp}/{safeFileName}";
                }
            }
            catch (WebException ex) when (ex.Response is FtpWebResponse ftpResponse)
            {
                _logger.LogError($"FTP error: {ftpResponse.StatusCode} - {ftpResponse.StatusDescription}");
                throw new InvalidOperationException($"FTP upload failed: {ftpResponse.StatusDescription}", ex);
            }
        }


        private async Task CreateFTPDirectoryAsync(string dirPath)
        {
            try
            {
                var request = (FtpWebRequest)WebRequest.Create(dirPath);
                request.Method = WebRequestMethods.Ftp.MakeDirectory;
                request.Credentials = new NetworkCredential(_ftpUsername, _ftpPassword);

                using (var response = (FtpWebResponse)await request.GetResponseAsync())
                {
                    _logger.LogInformation($"FTP directory created: {dirPath}");
                }
            }
            catch (WebException ex)
            {
                // Directory might already exist, that's ok
                if (ex.Response is FtpWebResponse response && response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                {
                    _logger.LogInformation($"FTP directory already exists: {dirPath}");
                }
                else
                {
                    _logger.LogWarning(ex, $"Could not create FTP directory: {dirPath}");
                }
            }
        }

        public async Task<bool> DeleteFileAsync(string filePath, StorageTypeEnum storageType)
        {
            try
            {
                switch (storageType)
                {
                    case StorageTypeEnum.Local:
                        var localPath = Path.Combine(_localStoragePath, filePath.Replace("/", "\\"));
                        if (File.Exists(localPath))
                        {
                            File.Delete(localPath);
                            return true;
                        }
                        break;

                    case StorageTypeEnum.UNC:
                        var uncPath = Path.Combine(_uncBasePath, filePath.Replace("/", "\\"));
                        if (File.Exists(uncPath))
                        {
                            File.Delete(uncPath);
                            return true;
                        }
                        break;

                    case StorageTypeEnum.FTP:
                        var ftpPath = $"{_ftpServer.TrimEnd('/')}/{filePath}";
                        var request = (FtpWebRequest)WebRequest.Create(ftpPath);
                        request.Method = WebRequestMethods.Ftp.DeleteFile;
                        request.Credentials = new NetworkCredential(_ftpUsername, _ftpPassword);

                        using (var response = (FtpWebResponse)await request.GetResponseAsync())
                        {
                            return response.StatusCode == FtpStatusCode.FileActionOK;
                        }
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to delete file {filePath} from {storageType}");
                return false;
            }
        }

        public async Task<bool> FileExistsAsync(string filePath, StorageTypeEnum storageType)
        {
            try
            {
                switch (storageType)
                {
                    case StorageTypeEnum.Local:
                        var localPath = Path.Combine(_localStoragePath, filePath.Replace("/", "\\"));
                        return File.Exists(localPath);

                    case StorageTypeEnum.UNC:
                        var uncPath = Path.Combine(_uncBasePath, filePath.Replace("/", "\\"));
                        return File.Exists(uncPath);

                    case StorageTypeEnum.FTP:
                        var ftpPath = $"{_ftpServer.TrimEnd('/')}/{filePath}";
                        var request = (FtpWebRequest)WebRequest.Create(ftpPath);
                        request.Method = WebRequestMethods.Ftp.GetFileSize;
                        request.Credentials = new NetworkCredential(_ftpUsername, _ftpPassword);

                        using (var response = (FtpWebResponse)await request.GetResponseAsync())
                        {
                            return response.StatusCode == FtpStatusCode.FileStatus;
                        }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public string GetFullPath(string relativePath, StorageTypeEnum storageType)
        {
            return storageType switch
            {
                StorageTypeEnum.Local => Path.Combine(_localStoragePath, relativePath.Replace("/", "\\")),
                StorageTypeEnum.UNC => Path.Combine(_uncBasePath, relativePath.Replace("/", "\\")),
                StorageTypeEnum.FTP => $"{_ftpServer.TrimEnd('/')}/{relativePath}",
                _ => relativePath
            };
        }

        private string GetSafeFileName(string fileName)
        {
            var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            var extension = Path.GetExtension(fileName);
            var safeBaseName = string.Join("_", nameWithoutExt.Split(Path.GetInvalidFileNameChars()));
            return $"{Guid.NewGuid()}_{safeBaseName}{extension}";
        }
    }
}