
using System.Threading.Tasks;

namespace HD.Station.MediaManagement.Mvc.Services
{
    public interface IFileProcessor
    {
        /// <summary>
        /// Chạy ffprobe để lấy metadata JSON.
        /// </summary>
        Task<string> GetMediaInfoJsonAsync(string filePath);

        /// <summary>
        /// Convert bất kỳ video đầu vào nào thành MP4 (H.264 + AAC).
        /// Trả về đường dẫn file MP4 đã convert.
        /// </summary>
        Task<string> ConvertToMp4Async(string inputPath, string outputFolder);
    }
}
