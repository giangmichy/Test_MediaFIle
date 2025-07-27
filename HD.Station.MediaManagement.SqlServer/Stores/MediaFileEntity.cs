using System;
using HD.Station.MediaManagement.Abstractions.Data;

namespace HD.Station.MediaManagement.SqlServer.Stores
{
    public class MediaFileEntity
    {
        public Guid Id { get; set; }
        public string FileName { get; set; }
        public MediaTypeEnum MediaType { get; set; }
        public long Size { get; set; }
        public FormatEnum Format { get; set; }
        public DateTime UploadTime { get; set; }
        public string StoragePath { get; set; }
        public string Description { get; set; }
        public StatusEnum Status { get; set; }
        public string MediaInfoJson { get; set; }
        public string Hash { get; set; }

        // Thêm thuộc tính storage mới
        public StorageTypeEnum StorageType { get; set; } = StorageTypeEnum.Local;
        public string? NetworkPath { get; set; }
    }
}