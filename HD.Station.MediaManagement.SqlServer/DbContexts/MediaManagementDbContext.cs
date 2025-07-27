using Microsoft.EntityFrameworkCore;
using HD.Station.MediaManagement.SqlServer.Stores;
using HD.Station.MediaManagement.Abstractions.Data;

namespace HD.Station.MediaManagement.SqlServer.DbContexts
{
    public class MediaManagementDbContext : DbContext
    {
        public MediaManagementDbContext(DbContextOptions<MediaManagementDbContext> options)
            : base(options)
        {
        }

        public DbSet<MediaFileEntity> MediaFiles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<MediaFileEntity>(b =>
            {
                b.ToTable("MediaFiles");
                b.HasKey(e => e.Id);

                b.Property(e => e.FileName)
                 .HasMaxLength(255)
                 .IsRequired();

                b.Property(e => e.StoragePath)
                 .HasMaxLength(500)
                 .IsRequired();

                b.Property(e => e.Description)
                 .HasMaxLength(1000);

                // Enum lưu dưới dạng string
                b.Property(e => e.MediaType)
                 .HasConversion<string>()
                 .IsRequired();

                b.Property(e => e.Format)
                 .HasConversion<string>()
                 .IsRequired();

                b.Property(e => e.Status)
                 .HasConversion<string>()
                 .IsRequired();

                b.Property(e => e.StorageType)
                 .HasConversion<string>()
                 .IsRequired();

                b.Property(e => e.NetworkPath)
                 .HasMaxLength(500)
                 .IsRequired(false);

                b.Property(e => e.MediaInfoJson)
                 .HasColumnType("nvarchar(max)");

                b.Property(e => e.Hash)
                 .HasMaxLength(100)
                 .IsRequired();
            });
        }
    }
}