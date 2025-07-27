using System;

namespace HD.Station.MediaManagement.Abstractions.Data
{
    public class MediaFileDto
    {
        public Guid Id { get; set; }
        public string? FileName { get; set; }
        public MediaTypeEnum MediaType { get; set; }
        public long Size { get; set; }
        public FormatEnum Format { get; set; }
        public DateTime UploadTime { get; set; }
        public string StoragePath { get; set; }
        public string? Description { get; set; }
        public StatusEnum Status { get; set; }
        public string MediaInfoJson { get; set; }
        public string Hash { get; set; }

        // Thêm thuộc tính mới cho storage type
        public StorageTypeEnum StorageType { get; set; } = StorageTypeEnum.Local;
        public string? NetworkPath { get; set; }  // Đường dẫn UNC hoặc FTP
    }
}