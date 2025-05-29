using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HD.Station.MediaManagement.Abstractions.Data;

namespace HD.Station.MediaManagement.Abstractions.Stores
{
    public interface IMediaFileStore
    {
        Task<IEnumerable<MediaFileDto>> ListAsync(string filter, int page, int size);
        Task<MediaFileDto> GetAsync(Guid id);
        Task<Guid> CreateAsync(MediaFileDto dto);
        Task UpdateAsync(MediaFileDto dto);
        Task DeleteAsync(Guid id);
    }
}
