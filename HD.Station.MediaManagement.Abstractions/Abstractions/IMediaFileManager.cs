using Microsoft.AspNetCore.Http;
using HD.Station.MediaManagement.Abstractions.Data;

namespace HD.Station.MediaManagement.Abstractions.Abstractions
{
    public interface IMediaFileManager
    {
        Task<IEnumerable<MediaFileDto>> ListAsync(string filter, int page, int size);
        Task<MediaFileDto> GetAsync(Guid id);
        Task<Guid> CreateAsync(IFormFile file, MediaFileDto dto);
        Task UpdateAsync(MediaFileDto dto);
        Task DeleteAsync(Guid id);
    }
}
