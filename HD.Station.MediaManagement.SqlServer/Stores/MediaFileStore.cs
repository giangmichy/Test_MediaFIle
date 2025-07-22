using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HD.Station.MediaManagement.Abstractions.Data;
using HD.Station.MediaManagement.Abstractions.Stores;
using HD.Station.MediaManagement.SqlServer.DbContexts;
using HD.Station.MediaManagement.SqlServer.Mapping;
using HD.Station.MediaManagement.SqlServer.Stores;

namespace HD.Station.MediaManagement.SqlServer.Stores
{
    public class MediaFileStore : IMediaFileStore
    {
        private readonly MediaManagementDbContext _db;

        public MediaFileStore(MediaManagementDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<MediaFileDto>> ListAsync(string filter, int page, int size)
        {
            var query = _db.MediaFiles.AsQueryable();

            // Chỉ hiển thị các file Active (không bao gồm Deleted)
            query = query.Where(e => e.Status != StatusEnum.Deleted);

            if (!string.IsNullOrWhiteSpace(filter))
                query = query.Where(e => e.FileName.Contains(filter) || e.Description.Contains(filter));

            var list = await query
                .OrderByDescending(e => e.UploadTime)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();

            return list.Select(e => e.ToDto());
        }

        public async Task<MediaFileDto> GetAsync(Guid id)
        {
            var entity = await _db.MediaFiles.FindAsync(id);
            return entity?.ToDto();
        }

        public async Task<Guid> CreateAsync(MediaFileDto dto)
        {
            // dto should already have Id, UploadTime, StoragePath, Hash, MediaInfoJson set
            var entity = dto.ToEntity();
            _db.MediaFiles.Add(entity);
            await _db.SaveChangesAsync();
            return entity.Id;
        }

        public async Task UpdateAsync(MediaFileDto dto)
        {
            var e = await _db.MediaFiles.FindAsync(dto.Id);
            if (e == null)
                throw new KeyNotFoundException($"MediaFile {dto.Id} not found.");

            // Cập nhật các trường cho phép sửa
            e.Description = dto.Description;
            e.MediaType = dto.MediaType;
            e.Status = dto.Status;
            // Giữ nguyên các trường khác: FileName, Format, Size, UploadTime, StoragePath, Hash, MediaInfoJson

            _db.MediaFiles.Update(e);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var e = await _db.MediaFiles.FindAsync(id);
            if (e == null)
                throw new KeyNotFoundException($"MediaFile {id} not found.");

            // Hard delete - xóa hẳn khỏi database
            _db.MediaFiles.Remove(e);
            await _db.SaveChangesAsync();
        }
    }
}