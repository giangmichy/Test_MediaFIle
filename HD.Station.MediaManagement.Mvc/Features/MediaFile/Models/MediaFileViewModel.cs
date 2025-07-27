using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using HD.Station.MediaManagement.Abstractions.Data;

namespace HD.Station.MediaManagement.Mvc.Features.MediaFile.Models
{
    public class MediaFileViewModel
    {
        public Guid Id { get; set; }

        [Display(Name = "File")]
        public IFormFile? FileUpload { get; set; }

        [Display(Name = "Tên file")]
        public string? FileName { get; set; }

        [Display(Name = "Loại media")]
        public MediaTypeEnum MediaType { get; set; }

        [Display(Name = "Định dạng")]
        public FormatEnum Format { get; set; }

        [Display(Name = "Kích thước (byte)")]
        public long Size { get; set; }

        [Display(Name = "Upload time")]
        public DateTime UploadTime { get; set; }

        [Display(Name = "Đường dẫn lưu")]
        public string StoragePath { get; set; } = string.Empty;

        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Display(Name = "Trạng thái")]
        public StatusEnum Status { get; set; } = StatusEnum.Active;

        // Thêm thuộc tính mới cho storage
        [Display(Name = "Phương thức lưu trữ")]
        public StorageTypeEnum StorageType { get; set; } = StorageTypeEnum.Local;

        [Display(Name = "Đường dẫn mạng")]
        public string? NetworkPath { get; set; }

        // Metadata
        public string? MediaInfoJson { get; set; }
        public string? Hash { get; set; }
    }
}