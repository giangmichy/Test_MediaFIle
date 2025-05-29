using HD.Station.MediaManagement.Abstractions.Data;
using HD.Station.MediaManagement.SqlServer.Stores;

namespace HD.Station.MediaManagement.SqlServer.Mapping
{
    public static class MediaFileMapping
    {
        public static MediaFileDto ToDto(this MediaFileEntity e) => new MediaFileDto
        {
            Id = e.Id,
            FileName = e.FileName,
            MediaType = e.MediaType,
            Size = e.Size,
            Format = e.Format,
            UploadTime = e.UploadTime,
            StoragePath = e.StoragePath,
            Description = e.Description,
            Status = e.Status,
            MediaInfoJson = e.MediaInfoJson,
            Hash = e.Hash
        };

        public static MediaFileEntity ToEntity(this MediaFileDto dto) => new MediaFileEntity
        {
            Id = dto.Id,
            FileName = dto.FileName,
            MediaType = dto.MediaType,
            Size = dto.Size,
            Format = dto.Format,
            UploadTime = dto.UploadTime,
            StoragePath = dto.StoragePath,
            Description = dto.Description,
            Status = dto.Status,
            MediaInfoJson = dto.MediaInfoJson,
            Hash = dto.Hash
        };
    }
}
