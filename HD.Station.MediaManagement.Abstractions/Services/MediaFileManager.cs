using Microsoft.AspNetCore.Http;
using HD.Station.MediaManagement.Abstractions.Abstractions;
using HD.Station.MediaManagement.Abstractions.Stores;
using HD.Station.MediaManagement.Abstractions.Data;

namespace HD.Station.MediaManagement.Abstractions.Services
{
    public class MediaFileManager : IMediaFileManager
    {
        private readonly IMediaFileStore _store;

        public MediaFileManager(IMediaFileStore store)
        {
            _store = store;
        }

        public Task<IEnumerable<MediaFileDto>> ListAsync(string filter, int page, int size)
            => _store.ListAsync(filter, page, size);

        public Task<MediaFileDto> GetAsync(Guid id)
            => _store.GetAsync(id);

        public async Task<Guid> CreateAsync(IFormFile file, MediaFileDto dto)
        {
            // TODO: tính Hash, MediaInfo trước khi lưu
            dto.Id = Guid.NewGuid();
            dto.UploadTime = DateTime.UtcNow;
            return await _store.CreateAsync(dto);
        }

        public Task UpdateAsync(MediaFileDto dto)
            => _store.UpdateAsync(dto);

        public Task DeleteAsync(Guid id)
            => _store.DeleteAsync(id);
    }
}
