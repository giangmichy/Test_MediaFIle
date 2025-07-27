using System;
using HD.Station.MediaManagement.Abstractions.Data;

namespace HD.Station.MediaManagement.SqlServer.Stores
{
    public class MediaFileEntity
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public MediaTypeEnum MediaType { get; set; }
        public long Size { get; set; }
        public FormatEnum Format { get; set; }
        public DateTime UploadTime { get; set; }
        public string StoragePath { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public StatusEnum Status { get; set; }
        public string MediaInfoJson { get; set; } = string.Empty;
        public string Hash { get; set; } = string.Empty;

        // ✅ FIX: Set default value ở property level thay vì EF config
        public StorageTypeEnum StorageType { get; set; } = StorageTypeEnum.Local;
        public string? NetworkPath { get; set; }
    }
}