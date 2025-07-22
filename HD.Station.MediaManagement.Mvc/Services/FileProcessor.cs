using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace HD.Station.MediaManagement.Mvc.Services
{
    public class FileProcessor : IFileProcessor
    {
        private readonly string _ffmpegExe;
        private readonly string _ffprobeExe;

        public FileProcessor(IConfiguration config)
        {
            // Đọc từ appsettings.json: "FFmpegSettings": { "BinFolder": "..."}
            var bin = config["FFmpegSettings:BinFolder"]
                      ?? throw new InvalidOperationException("FFmpegSettings:BinFolder missing");
            _ffmpegExe = Path.Combine(bin, "ffmpeg.exe");
            _ffprobeExe = Path.Combine(bin, "ffprobe.exe");
        }

        public async Task<string> GetMediaInfoJsonAsync(string filePath)
        {
            var psi = new ProcessStartInfo
            {
                FileName = _ffprobeExe,
                Arguments = $"-v quiet -print_format json -show_format -show_streams \"{filePath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var p = Process.Start(psi)!;
            var json = await p.StandardOutput.ReadToEndAsync();
            var err = await p.StandardError.ReadToEndAsync();
            await p.WaitForExitAsync();
            if (p.ExitCode != 0)
                throw new InvalidOperationException($"ffprobe failed: {err}");
            return json;
        }

        public async Task<string> ConvertToMp4Async(string inputPath, string outputFolder)
        {
            Directory.CreateDirectory(outputFolder);

            var outFile = Path.Combine(
                outputFolder,
                Path.GetFileNameWithoutExtension(inputPath) + ".mp4"
            );
            if (File.Exists(outFile))
                File.Delete(outFile);

            // Chuẩn H.264 + AAC
            var args = $"-y -i \"{inputPath}\" -c:v libx264 -preset fast -crf 23 -c:a aac -b:a 128k \"{outFile}\"";
            var psi = new ProcessStartInfo
            {
                FileName = _ffmpegExe,
                Arguments = args,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var p = Process.Start(psi)!;
            var err = await p.StandardError.ReadToEndAsync();
            await p.WaitForExitAsync();
            if (p.ExitCode != 0)
                throw new InvalidOperationException($"ffmpeg failed: {err}");
            return outFile;
        }
    }
}
