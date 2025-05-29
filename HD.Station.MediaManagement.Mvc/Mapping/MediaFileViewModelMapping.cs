using HD.Station.MediaManagement.Abstractions.Data;
using HD.Station.MediaManagement.Mvc.Features.MediaFile.Models;

namespace HD.Station.MediaManagement.Mvc.Mapping
{
    public static class MediaFileViewModelMapping
    {
        public static MediaFileViewModel ToViewModel(this MediaFileDto dto)
        {
            if (dto == null) return null;
            return new MediaFileViewModel
            {
                Id = dto.Id,
                FileName = dto.FileName,
                MediaType = dto.MediaType,
                Format = dto.Format,
                Size = dto.Size,
                UploadTime = dto.UploadTime,
                StoragePath = dto.StoragePath,
                Description = dto.Description,
                Status = dto.Status,
                MediaInfoJson = dto.MediaInfoJson,
                Hash = dto.Hash
            };
        }

        public static MediaFileDto ToDto(this MediaFileViewModel vm)
        {
            if (vm == null) return null;
            return new MediaFileDto
            {
                Id = vm.Id,
                FileName = vm.FileName,
                MediaType = vm.MediaType,
                Format = vm.Format,
                Size = vm.Size,
                UploadTime = vm.UploadTime,
                StoragePath = vm.StoragePath,
                Description = vm.Description,
                Status = vm.Status,
                MediaInfoJson = vm.MediaInfoJson,
                Hash = vm.Hash
            };
        }
    }
}
