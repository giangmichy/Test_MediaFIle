using HD.Station.MediaManagement.Abstractions.Data;
using System.IO;
using System.Threading.Tasks;

namespace HD.Station.MediaManagement.Mvc.Services
{
    public interface IFileStorageService
    {
        /// <summary>
        /// Upload file theo storage type được chọn
        /// </summary>
        Task<string> UploadFileAsync(Stream fileStream, string fileName, StorageTypeEnum storageType);

        /// <summary>
        /// Xóa file theo storage type
        /// </summary>
        Task<bool> DeleteFileAsync(string filePath, StorageTypeEnum storageType);

        /// <summary>
        /// Kiểm tra file có tồn tại không
        /// </summary>
        Task<bool> FileExistsAsync(string filePath, StorageTypeEnum storageType);

        /// <summary>
        /// Get đường dẫn đầy đủ cho file
        /// </summary>
        string GetFullPath(string relativePath, StorageTypeEnum storageType);
    }
}